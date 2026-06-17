using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sequencing.Core;
using Unity.Netcode;
using UnityEngine;

namespace Multiplayer.Sequencing
{
    /// <summary>
    /// A synchronization gate. Every peer that reaches this step reports in to the
    /// server and then waits; the chain only continues once every connected peer
    /// has arrived. Drop it in front of anything that needs everyone present and in
    /// lockstep, e.g. a synchronized cutscene.
    /// </summary>
    public class NetcodeSyncSequence : NetworkBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour next;

        // Server-only: which peers have reached the gate.
        private readonly HashSet<ulong> _reportedClients = new();

        // Completed on every peer once the server releases the gate.
        private UniTaskCompletionSource _barrier;

        public IEntrySequence Default => next as IEntrySequence;
        public bool IsCompleted => false;
        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            // Not in a network session (offline / single player): nothing to wait on.
            if (!IsSpawned)
                return Default;

            _barrier = new UniTaskCompletionSource();

            if (IsServer)
            {
                _reportedClients.Clear();
                // A disconnect lowers how many we are waiting on, which may now
                // satisfy the gate, so re-check whenever one happens.
                NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
                // Covers the zero-client case (e.g. a dedicated server with nobody yet).
                TryReleaseBarrier();
            }

            DisplayMessage?.Invoke("Waiting for all players...");

            // The host counts as a client and reports here. A dedicated server does
            // not report for itself; it only coordinates and waits for the release.
            if (IsClient)
                ReportReadyRpc();

            await _barrier.Task;

            if (IsServer)
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;

            return Default;
        }

        // Any peer -> server. Records that the sender has reached the gate.
        [Rpc(SendTo.Server)]
        private void ReportReadyRpc(RpcParams rpcParams = default)
        {
            _reportedClients.Add(rpcParams.Receive.SenderClientId);
            TryReleaseBarrier();
        }

        // Server-only. Releases once every connected client has reported.
        private void TryReleaseBarrier()
        {
            if (!IsServer) return;
            if (_reportedClients.Count < NetworkManager.ConnectedClientsIds.Count) return;

            // Goes to every peer including this server, so each one's await completes.
            ReleaseBarrierRpc();
        }

        // Server -> everyone (server included). Completes the local gate.
        [Rpc(SendTo.Everyone)]
        private void ReleaseBarrierRpc()
        {
            _barrier?.TrySetResult();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            _reportedClients.Remove(clientId);
            TryReleaseBarrier();
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager != null)
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;

            // Don't strand the loading chain if we despawn while still waiting.
            _barrier?.TrySetResult();

            base.OnNetworkDespawn();
        }

        private void OnDrawGizmos()
        {
            if (next && Default == null)
                Debug.LogError("Success is INVALID", gameObject);
        }
    }
}