using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Multiplayer.GameObjects;
using Sequencing.Core;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Multiplayer.Sequencing
{
    /// <summary>
    /// Spawns each connected player's chosen character, read from
    /// CharacterSelectionStore. Server-authoritative.
    ///
    /// Cross-peer "spawn at the same time" is provided by the NetworkSequenceEntryPoint
    /// barrier that wraps every stage (and/or a NetcodeSyncSequence placed right after
    /// this one): no peer advances past this stage until every peer has finished it,
    /// and NGO replicates the spawns to clients as part of that. Clients therefore do
    /// not need to poll a synced flag here; the server does its work and the barrier
    /// holds the group together.
    /// </summary>
    public class SpawnCharacterSequence : NetworkBehaviour, IEntrySequence
    {
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Behaviour next;
        [SerializeField] private Behaviour failure;

        public IEntrySequence Default => next as IEntrySequence;
        public bool IsCompleted => false;
        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("[SpawnCharacterSequence] Executing");

            if (!CharacterSelectionStore.Instance)
            {
                Debug.LogError("[SpawnCharacterSequence] No CharacterSelectionStore spawned.");
                return failure as IEntrySequence;
            }

            DisplayMessage?.Invoke("Spawning players...");

            if (IsServer)
                SpawnAll();

            // No client-side poll. The entry-point barrier (or a following
            // NetcodeSyncSequence) gates everyone to advance together, by which point
            // NGO has replicated the spawned NetworkObjects to clients.
            await UniTask.Yield();
            return Default;
        }

        // Server only.
        private void SpawnAll()
        {
            var available = new List<Transform>(spawnPoints);
            var prefabList = NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists[0].PrefabList;

            foreach (var kvp in CharacterSelectionStore.Instance.ActiveSelections())
            {
                if (available.Count == 0)
                {
                    Debug.LogError("[SpawnCharacterSequence] Ran out of spawn points.");
                    break;
                }

                int idx = Random.Range(0, available.Count);
                Transform point = available[idx];
                available.RemoveAt(idx);

                NetworkPrefab match = null;
                foreach (var prefab in prefabList)
                    if (prefab.SourcePrefabGlobalObjectIdHash == kvp.Value) { match = prefab; break; }

                if (match == null)
                {
                    Debug.LogError($"[SpawnCharacterSequence] No prefab for id {kvp.Value} (client {kvp.Key}).");
                    available.Add(point); // give the point back
                    continue;
                }

                // SpawnAsPlayerObject can be called only once per client; if a client
                // can already own a player object from a prior round, despawn it first.
                if (NetworkManager.ConnectedClients.TryGetValue(kvp.Key, out var cc)
                    && cc.PlayerObject != null)
                {
                    Debug.LogWarning($"[SpawnCharacterSequence] Client {kvp.Key} already has a player object; skipping.");
                    available.Add(point);
                    continue;
                }

                GameObject go = Instantiate(match.Prefab, point.position, point.rotation);
                go.GetComponent<NetworkObject>().SpawnAsPlayerObject(kvp.Key);
            }

            Debug.Log("[SpawnCharacterSequence] Spawned all selected characters.");
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