using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Multiplayer.GameObjects;
using Sequencing.Core;
using Unity.Netcode;
using UnityEngine;
#if UNITY_SERVICES
using Unity.Services.Authentication;
#endif

namespace Multiplayer.Sequencing
{
    /// <summary>
    /// Runs the character-selection window. The server owns the timer and decides
    /// when selection is finished; clients just wait on the synced flag so every peer
    /// exits this stage together (the entry-point barrier then gates the next stage).
    ///
    /// The timer is fully networked: the server counts down a NetworkVariable, and
    /// every peer (server included) reads it each frame and pushes the normalized
    /// 1 -> 0 value into the active ICharacterSelector, so all players see the same
    /// bar. The window closes when the timer hits zero OR every connected client has
    /// already picked, whichever comes first.
    ///
    /// At close the server asks each peer for the character it was last looking at,
    /// then auto-assigns: a non-picker gets its hovered character if still free, else
    /// a random free one. Everything routes through CharacterSelectionStore.TrySelect,
    /// so no two players can end up with the same character. A SelectionComplete RPC
    /// then fires on every peer.
    ///
    /// Place this AFTER NetcodeSigninSequence (PlayerId must exist) and after the
    /// store has been spawned.
    /// </summary>
    public class SelectCharacterSequence : NetworkBehaviour, IEntrySequence
    {
        [SerializeField] private float selectionSeconds = 30f;
        // How long the server waits for hover replies after asking, before it just
        // auto-assigns with whatever hovers it has (random for the rest).
        [SerializeField] private float hoverGatherSeconds = 0.5f;
        // Prefab-id hashes of every selectable character. Used as the random pool for
        // auto-assign. Fill from the same characters your PenguinSelectors point at.
        [SerializeField] private ulong[] selectablePrefabIds;
        [SerializeField] private Behaviour next;
        [SerializeField] private Behaviour failure;

        private readonly NetworkVariable<bool> _isSelectionDone = new();
        private readonly NetworkVariable<float> _selectionTime = new();
        private readonly NetworkVariable<float> _selectionDuration = new();

        // Server-only: clientId -> last-hovered prefab-id, collected at window close.
        private readonly Dictionary<ulong, ulong> _hovers = new();

        public IEntrySequence Default => next as IEntrySequence;
        public bool IsCompleted => false;
        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("[SelectCharacterSequence] Executing");

            if (!CharacterSelectionStore.Instance)
            {
                Debug.LogError("[SelectCharacterSequence] No CharacterSelectionStore spawned.");
                return failure as IEntrySequence;
            }

            if (CharacterSelector.Active == null)
            {
                Debug.LogError("[SelectCharacterSequence] No active ICharacterSelector.");
                return failure as IEntrySequence;
            }

            CharacterSelector.Active.LocalCharacterChosen += OnLocalCharacterChosen;

            DisplayMessage?.Invoke("Selecting characters...");

            if (IsServer)
            {
                CharacterSelectionStore.Instance.RegisterFromConnected();

                _selectionDuration.Value = selectionSeconds;
                _selectionTime.Value = selectionSeconds;

                // Run until the timer expires or everyone has already picked.
                while (_selectionTime.Value > 0f && !EveryoneHasPicked())
                {
                    _selectionTime.Value -= Time.deltaTime;
                    PushFill();
                    await UniTask.Yield();
                }

                _selectionTime.Value = 0f;
                PushFill();

                // Ask peers what they were hovering, give them a moment to reply.
                _hovers.Clear();
                RequestHover_ClientRpc();
                float deadline = Time.realtimeSinceStartup + hoverGatherSeconds;
                while (Time.realtimeSinceStartup < deadline && !AllHoversIn())
                    await UniTask.Yield();

                AutoAssignMissing();
                _isSelectionDone.Value = true;

                // Tell everyone the window is done (requirement 1).
                SelectionComplete_ClientRpc();
            }
            else
            {
                while (!_isSelectionDone.Value)
                {
                    PushFill();
                    await UniTask.Yield();
                }
            }

            PushFill();
            CharacterSelector.Active.SelectionFinished();
            CharacterSelector.Active.LocalCharacterChosen -= OnLocalCharacterChosen;
            return Default;
        }

        private bool EveryoneHasPicked()
        {
            int picked = 0;
            foreach (var _ in CharacterSelectionStore.Instance.ActiveSelections()) picked++;
            return picked >= NetworkManager.ConnectedClientsIds.Count;
        }

        private bool AllHoversIn()
        {
            foreach (var clientId in NetworkManager.ConnectedClientsIds)
                if (!_hovers.ContainsKey(clientId)) return false;
            return true;
        }

        private void PushFill()
        {
            float dur = _selectionDuration.Value;
            float fill = dur > 0f ? Mathf.Clamp01(_selectionTime.Value / dur) : 0f;
            CharacterSelector.Active?.SetTimeRemaining(fill);
        }

        // ---- Hover gathering ----

        // Server -> everyone: report your current hover.
        [Rpc(SendTo.Everyone)]
        private void RequestHover_ClientRpc()
        {
            ulong hover = CharacterSelector.Active?.CurrentHoverPrefabId ?? 0;
            ReportHover_ServerRpc(hover);
        }

        // Any peer -> server: my current hover.
        [Rpc(SendTo.Server)]
        private void ReportHover_ServerRpc(ulong hoverPrefabId, RpcParams p = default)
        {
            _hovers[p.Receive.SenderClientId] = hoverPrefabId;
        }

        // ---- Auto-assign (server) ----

        private void AutoAssignMissing()
        {
            var store = CharacterSelectionStore.Instance;

            var taken = new HashSet<ulong>();
            foreach (var t in store.TakenList) taken.Add(t.PrefabId);

            var picked = new HashSet<ulong>();
            foreach (var kvp in store.ActiveSelections()) picked.Add(kvp.Key);

            foreach (var clientId in NetworkManager.ConnectedClientsIds)
            {
                if (picked.Contains(clientId)) continue;

                if (!store.TryGetPlayerId(clientId, out string playerId))
                    playerId = clientId.ToString();

                // Prefer the character the player was last looking at, if it is free.
                ulong chosen = 0;
                if (_hovers.TryGetValue(clientId, out ulong hover)
                    && hover != 0 && !taken.Contains(hover))
                {
                    chosen = hover;
                }
                else
                {
                    // Otherwise a random free one.
                    var free = new List<ulong>();
                    foreach (var id in selectablePrefabIds)
                        if (!taken.Contains(id)) free.Add(id);

                    if (free.Count == 0)
                    {
                        Debug.LogError($"[SelectCharacterSequence] No free character to auto-assign to client {clientId}.");
                        continue;
                    }
                    chosen = free[UnityEngine.Random.Range(0, free.Count)];
                }

                if (store.TrySelect(clientId, playerId, "Player", chosen))
                {
                    taken.Add(chosen); // keep the local view in sync so the next loop won't reuse it
                    Debug.Log($"[SelectCharacterSequence] Auto-assigned {chosen} to client {clientId}.");
                }
                else
                {
                    Debug.LogError($"[SelectCharacterSequence] Auto-assign of {chosen} to client {clientId} was rejected.");
                }
            }
        }

        // ---- Picking ----

        private void OnLocalCharacterChosen(ulong prefabId)
        {
            Debug.Log($"[SelectCharacterSequence] Local pick {prefabId}");
            SelectCharacter_ServerRpc(prefabId, LocalPlayerId(), LocalPlayerName());
        }

        [Rpc(SendTo.Server)]
        private void SelectCharacter_ServerRpc(ulong prefabId, string playerId, string playerName,
                                               RpcParams p = default)
        {
            bool ok = CharacterSelectionStore.Instance.TrySelect(
                p.Receive.SenderClientId, playerId, playerName, prefabId);

            if (!ok)
                RejectSelection_ClientRpc(prefabId,
                    RpcTarget.Single(p.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void RejectSelection_ClientRpc(ulong prefabId, RpcParams p)
        {
            Debug.Log($"[SelectCharacterSequence] Pick {prefabId} rejected (already taken)");
            CharacterSelector.Active?.OnSelectionRejected(prefabId);
        }

        // Server -> everyone: the selection window has finished (requirement 1).
        [Rpc(SendTo.Everyone)]
        private void SelectionComplete_ClientRpc()
        {
            Debug.Log("[SelectCharacterSequence] Selection complete (all peers notified).");
            CharacterSelector.Active?.SelectionFinished();
        }

        private static string LocalPlayerId()
        {
#if UNITY_SERVICES
            if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
                return AuthenticationService.Instance.PlayerId;
#endif
            return NetworkManager.Singleton != null
                ? NetworkManager.Singleton.LocalClientId.ToString()
                : string.Empty;
        }

        private static string LocalPlayerName()
        {
#if UNITY_SERVICES
            if (AuthenticationService.Instance != null)
            {
                string n = AuthenticationService.Instance.PlayerName;
                if (!string.IsNullOrEmpty(n)) return n;
            }
#endif
            return "Player";
        }

        private void OnDrawGizmos()
        {
            if (next && Default == null)
                Debug.LogError("Success is INVALID", gameObject);
            if (failure && failure is not IEntrySequence)
                Debug.LogError("Failure is INVALID", gameObject);
        }
    }
}