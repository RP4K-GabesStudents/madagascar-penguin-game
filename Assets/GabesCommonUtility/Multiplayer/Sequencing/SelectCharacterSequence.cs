using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Multiplayer.GameObjects;
using Sequencing.Core;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;

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

        // Builds the full pool from the registered network prefabs (the authoritative
        // source SpawnCharacterSequence also uses), subtracts what's already taken,
        // and gives every non-picker a free character: their hover if it's still free,
        // otherwise the next free one. Pure prefab-id logic, so it never depends on the
        // client-id bookkeeping being perfectly in sync. With N characters and <= N
        // players this always fills.
        private void AutoAssignMissing()
        {
            var store = CharacterSelectionStore.Instance;

            // What's already claimed (prefab-ids).
            var taken = new HashSet<ulong>();
            foreach (var t in store.TakenList) taken.Add(t.PrefabId);

            // Full set of selectable character prefab-ids, from the network prefab list.
            var allCharacters = AllCharacterPrefabIds();

            // Which connected clients already hold a pick (prefab-id, not client-id,
            // so a stale mapping can't make us double-assign a taken character).
            var pickedClients = new HashSet<ulong>();
            foreach (var kvp in store.ActiveSelections()) pickedClients.Add(kvp.Key);

            foreach (var clientId in NetworkManager.ConnectedClientsIds)
            {
                if (pickedClients.Contains(clientId)) continue;

                if (!store.TryGetPlayerId(clientId, out string playerId))
                    playerId = clientId.ToString();

                // Resolve the auto-assigned player's real name from the lobby roster.
                string resolvedName = PlayerNameDirectory.ResolveOr(playerId, "Player");

                ulong chosen = 0;

                // Prefer the hovered character if it's still free.
                if (_hovers.TryGetValue(clientId, out ulong hover)
                    && hover != 0 && !taken.Contains(hover))
                {
                    chosen = hover;
                }
                else
                {
                    // First free character from the authoritative pool.
                    foreach (var id in allCharacters)
                        if (!taken.Contains(id)) { chosen = id; break; }
                }

                if (chosen == 0)
                {
                    // Only reachable if there are genuinely more players than
                    // characters. Logged loud so the lobby-size/character-count
                    // mismatch is obvious rather than a silent no-spawn.
                    Debug.LogError($"[SelectCharacterSequence] No free character for client {clientId}: " +
                                   $"{allCharacters.Count} characters, all taken. Check lobby size vs character count.");
                    continue;
                }

                if (store.TrySelect(clientId, playerId, resolvedName, chosen))
                {
                    // resolvedName comes from the lobby roster; "Player" only if the
                    // directory has no name for this id.
                    taken.Add(chosen);
                    Debug.Log($"[SelectCharacterSequence] Auto-assigned {chosen} to client {clientId}.");
                }
                else
                {
                    Debug.LogError($"[SelectCharacterSequence] Auto-assign of {chosen} to client {clientId} was rejected.");
                }
            }
        }

        // Authoritative list of every selectable character's prefab-id hash, read from
        // the same network prefab list SpawnCharacterSequence spawns from. If you have
        // non-character network prefabs registered, set selectablePrefabIds in the
        // inspector and they take precedence as an explicit allow-list.
        private List<ulong> AllCharacterPrefabIds()
        {
            if (selectablePrefabIds != null && selectablePrefabIds.Length > 0)
                return new List<ulong>(selectablePrefabIds);

            var ids = new List<ulong>();
            var prefabList = NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists[0].PrefabList;
            foreach (var prefab in prefabList)
                ids.Add(prefab.SourcePrefabGlobalObjectIdHash);
            return ids;
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
            // Resolve the display name server-side from the lobby roster by playerId;
            // fall back to whatever the client sent, then to "Player".
            string resolved = PlayerNameDirectory.ResolveOr(playerId,
                string.IsNullOrEmpty(playerName) ? "Player" : playerName);

            bool ok = CharacterSelectionStore.Instance.TrySelect(
                p.Receive.SenderClientId, playerId, resolved, prefabId);

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

        private static bool IsAuthReady()
            => UnityServices.State == ServicesInitializationState.Initialized
               && AuthenticationService.Instance != null
               && AuthenticationService.Instance.IsAuthorized;

        private static string LocalPlayerId()
        {
            if (IsAuthReady() && AuthenticationService.Instance.IsSignedIn)
                return AuthenticationService.Instance.PlayerId;

            return NetworkManager.Singleton != null
                ? NetworkManager.Singleton.LocalClientId.ToString()
                : string.Empty;
        }

        private static string LocalPlayerName()
        {
            if (IsAuthReady())
            {
                string n = AuthenticationService.Instance.PlayerName;
                if (!string.IsNullOrEmpty(n)) return n;
            }
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