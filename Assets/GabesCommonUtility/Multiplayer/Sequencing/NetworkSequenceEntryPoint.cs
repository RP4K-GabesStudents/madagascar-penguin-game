using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sequencing.Managers;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Multiplayer.Sequencing
{
    /*
     * Runs the same IEntrySequence chain as SequenceEntryPoint, but after every
     * stage holds a server-coordinated barrier: no peer advances to stage N+1 until
     * every connected peer has finished stage N.
     *
     * The server is authoritative on which chain is correct:
     *  - On connect a client sends a Hello with its chain signature. A mismatch is
     *    logged in full detail and the client is disconnected.
     *  - A peer that joins mid-sequence replays the chain from the start; stages the
     *    group has already cleared are released to it immediately, so it catches up
     *    and rejoins the live barrier. The group holds at the current frontier until
     *    the joiner reaches it (the per-stage timeout is the backstop).
     *
     * Uses CustomMessagingManager named messages rather than a NetworkBehaviour so
     * the coordinator can be a plain persisting MonoBehaviour that survives the scene
     * loads it is coordinating. For joins that land after OnFinish, keep persist = true
     * and do not tear this down at completion.
     *
     * Setup: set persist = true, and either set activateOnStart = false and call
     * ActivateAndForget() from your connection flow, or place this in a scene that
     * only loads after the session is up.
     */
    public class NetworkSequenceEntryPoint : SequenceEntryPoint
    {
        private const string HelloMessage = "SeqChain.Hello";
        private const string RejectMessage = "SeqChain.Reject";
        private const string ReachedMessage = "SeqBarrier.Reached";
        private const string ReleaseMessage = "SeqBarrier.Release";

        [SerializeField] private float barrierTimeout = 30f;

        // Server: stage -> set of peer ids that have reported finishing it.
        private readonly Dictionary<int, HashSet<ulong>> _reached = new();
        // Any peer: stages that have been released (lets a late wait return at once).
        private readonly HashSet<int> _released = new();
        // Any peer: stage -> completion source signalled when the server releases it.
        private readonly Dictionary<int, UniTaskCompletionSource> _waits = new();
        // Server: peers whose chain matched and whose reports are therefore counted.
        private readonly HashSet<ulong> _validated = new();

        private bool _handlersRegistered;

        private static NetworkManager Net => NetworkManager.Singleton;
        private static bool HasSession => Net && (Net.IsServer || Net.IsClient);

        // Base has no OnEnable/OnDisable, so these don't hide anything. Cleanup lives
        // here rather than OnDestroy because redeclaring the base's private OnDestroy
        // would suppress its loop cancellation.
        private void OnEnable()
        {
            if (!Net) return;
            Net.OnServerStarted += OnSessionUp;
            Net.OnClientConnectedCallback += OnClientConnected;
            if (HasSession) EnsureRegistered();
        }

        private void OnDisable() => Unregister();

        private void OnSessionUp() => EnsureRegistered();

        private void OnClientConnected(ulong clientId)
        {
            if (Net && clientId == Net.LocalClientId) EnsureRegistered();
        }

        private void EnsureRegistered()
        {
            if (_handlersRegistered || !HasSession) return;

            var cm = Net.CustomMessagingManager;

            if (Net.IsServer)
            {
                cm.RegisterNamedMessageHandler(HelloMessage, OnHelloReceived);
                cm.RegisterNamedMessageHandler(ReachedMessage, OnReachedReceived);
                Net.OnClientDisconnectCallback += OnClientDisconnect;
                _validated.Add(NetworkManager.ServerClientId); // the server trusts itself
            }
            else
            {
                cm.RegisterNamedMessageHandler(ReleaseMessage, OnReleaseReceived);
                cm.RegisterNamedMessageHandler(RejectMessage, OnRejectReceived);
                SendHello(); // sent before any Reached, so the server validates us first
            }

            _handlersRegistered = true;
        }

        private void Unregister()
        {
            if (Net)
            {
                Net.OnServerStarted -= OnSessionUp;
                Net.OnClientConnectedCallback -= OnClientConnected;
            }

            if (!_handlersRegistered) return;

            if (Net)
            {
                var cm = Net.CustomMessagingManager;
                if (Net.IsServer)
                {
                    cm?.UnregisterNamedMessageHandler(HelloMessage);
                    cm?.UnregisterNamedMessageHandler(ReachedMessage);
                    Net.OnClientDisconnectCallback -= OnClientDisconnect;
                }
                else
                {
                    cm?.UnregisterNamedMessageHandler(ReleaseMessage);
                    cm?.UnregisterNamedMessageHandler(RejectMessage);
                }
            }

            _handlersRegistered = false;
        }

        protected override async UniTask OnStageComplete(int stage, CancellationToken token)
        {
            // No session: behave exactly like the base (single-player / offline).
            if (!HasSession) return;

            EnsureRegistered();

            if (Net.IsServer)
            {
                Report(stage, NetworkManager.ServerClientId);
                TryRelease(stage);
            }
            else
            {
                SendReached(stage);
            }

            try
            {
                await WaitForRelease(stage)
                    .AttachExternalCancellation(token)
                    .Timeout(TimeSpan.FromSeconds(barrierTimeout));
            }
            catch (TimeoutException)
            {
                Debug.LogError($"[NetworkSequence] loading barrier timed out at stage {stage}; proceeding without full sync.", this);
            }
        }

        // ---- Validation (server) ----

        private void OnHelloReceived(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out long theirSignature);

            if (theirSignature == ChainSignature)
            {
                _validated.Add(senderClientId);
                return;
            }

            RejectClient(senderClientId, theirSignature);
        }

        private void RejectClient(ulong clientId, long theirSignature)
        {
            Debug.LogError(
                $"[NetworkSequence] SEVERE: client {clientId} has a mismatched loading chain and is being disconnected.\n" +
                $"  Expected signature : 0x{ChainSignature:X16}\n" +
                $"  Received signature : 0x{theirSignature:X16}\n" +
                $"  Canonical chain:\n{DescribeCanonicalChain()}", this);

            SendRejectTo(clientId);                  // best-effort reason for the client
            DisconnectAfterFlush(clientId).Forget();  // disconnect next frame so it flushes
        }

        private async UniTaskVoid DisconnectAfterFlush(ulong clientId)
        {
            await UniTask.NextFrame();
            if (Net && Net.IsServer && Net.ConnectedClients.ContainsKey(clientId))
                Net.DisconnectClient(clientId);
        }

        private string DescribeCanonicalChain()
        {
            var sb = new StringBuilder();
            int i = 0;
            var current = StartSequence;
            while (current != null)
            {
                i++;
                string where = current is Component c ? $" (on '{c.gameObject.name}')" : "";
                sb.AppendLine($"    #{i} {current.GetType().FullName}{where}");
                current = current.Default;
            }
            return sb.ToString();
        }

        // ---- Validation (client) ----

        private void OnRejectReceived(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out long expected);
            Debug.LogError(
                "[NetworkSequence] SEVERE: the server rejected our loading chain and is disconnecting us.\n" +
                $"  Server signature : 0x{expected:X16}\n" +
                $"  Our signature    : 0x{ChainSignature:X16}\n" +
                "  This usually means a version or content mismatch with the host.", this);
        }

        // ---- Barrier bookkeeping (server) ----

        private void Report(int stage, ulong clientId)
        {
            if (!_reached.TryGetValue(stage, out var set))
                _reached[stage] = set = new HashSet<ulong>();
            set.Add(clientId);
        }

        private void OnReachedReceived(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int stage);

            if (!_validated.Contains(senderClientId)) return; // ignore until the Hello validated them

            // Catch-up: a stage the group already cleared. Release this one client now.
            if (_released.Contains(stage))
            {
                SendReleaseTo(senderClientId, stage);
                return;
            }

            Report(stage, senderClientId);
            TryRelease(stage);
        }

        private void TryRelease(int stage)
        {
            if (_released.Contains(stage)) return;
            if (!_reached.TryGetValue(stage, out var set)) return;

            // The server's own stage must be done. Covers the dedicated-server case,
            // where the server id is not part of ConnectedClientsIds.
            if (!set.Contains(NetworkManager.ServerClientId)) return;

            foreach (var id in Net.ConnectedClientsIds)
                if (!set.Contains(id)) return; // someone is still loading

            _released.Add(stage);
            Broadcast(stage);
            SignalRelease(stage); // release the server's own wait (ToAll skips the sender)
        }

        private void OnClientDisconnect(ulong clientId)
        {
            _validated.Remove(clientId);
            foreach (var set in _reached.Values) set.Remove(clientId);

            // A peer leaving shrinks the denominator and can complete a stuck barrier.
            foreach (var stage in new List<int>(_reached.Keys))
                TryRelease(stage);
        }

        // ---- Messaging ----

        private void SendHello()
        {
            using var writer = new FastBufferWriter(sizeof(long), Allocator.Temp);
            writer.WriteValueSafe(ChainSignature);
            Net.CustomMessagingManager.SendNamedMessage(HelloMessage, NetworkManager.ServerClientId, writer, NetworkDelivery.ReliableSequenced);
        }

        private void SendRejectTo(ulong clientId)
        {
            using var writer = new FastBufferWriter(sizeof(long), Allocator.Temp);
            writer.WriteValueSafe(ChainSignature);
            Net.CustomMessagingManager.SendNamedMessage(RejectMessage, clientId, writer, NetworkDelivery.ReliableSequenced);
        }

        private void SendReached(int stage)
        {
            using var writer = new FastBufferWriter(sizeof(int), Allocator.Temp);
            writer.WriteValueSafe(stage);
            Net.CustomMessagingManager.SendNamedMessage(ReachedMessage, NetworkManager.ServerClientId, writer, NetworkDelivery.ReliableSequenced);
        }

        private void Broadcast(int stage)
        {
            using var writer = new FastBufferWriter(sizeof(int), Allocator.Temp);
            writer.WriteValueSafe(stage);
            Net.CustomMessagingManager.SendNamedMessageToAll(ReleaseMessage, writer, NetworkDelivery.ReliableSequenced);
        }

        private void SendReleaseTo(ulong clientId, int stage)
        {
            using var writer = new FastBufferWriter(sizeof(int), Allocator.Temp);
            writer.WriteValueSafe(stage);
            Net.CustomMessagingManager.SendNamedMessage(ReleaseMessage, clientId, writer, NetworkDelivery.ReliableSequenced);
        }

        private void OnReleaseReceived(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int stage);
            SignalRelease(stage);
        }

        // ---- Wait / release (any peer) ----

        private UniTask WaitForRelease(int stage)
        {
            if (_released.Contains(stage)) return UniTask.CompletedTask;

            if (!_waits.TryGetValue(stage, out var tcs))
                _waits[stage] = tcs = new UniTaskCompletionSource();
            return tcs.Task;
        }

        private void SignalRelease(int stage)
        {
            _released.Add(stage);
            if (_waits.TryGetValue(stage, out var tcs))
            {
                tcs.TrySetResult();
                _waits.Remove(stage);
            }
        }
    }
}