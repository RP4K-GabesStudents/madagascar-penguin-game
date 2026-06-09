using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using NetworkPrefab = Unity.Netcode.NetworkPrefab;

namespace Managers.Game
{
    /// <summary>
    /// Tracks each player's chosen character prefab hash and spawns
    /// the correct networked prefab at a random spawn point.
    /// Must run on the server only.
    /// </summary>
    public class CharacterSpawner : NetworkBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField] private List<Transform> spawnPoints;

        // clientId → chosen prefab hash
        private readonly Dictionary<ulong, ulong> _playerChoices = new();

        /// <summary>How many players have registered a choice so far.</summary>
        public int ChoiceCount => _playerChoices.Count;

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Records or updates a player's prefab choice.
        /// Safe to call multiple times for the same client (latest choice wins).
        /// </summary>
        public void RegisterChoice(ulong clientId, ulong prefabHash)
        {
            if (!_playerChoices.TryAdd(clientId, prefabHash))
                _playerChoices[clientId] = prefabHash;
        }

        /// <summary>
        /// Spawns every registered player's chosen character.
        /// Consumes the available spawn points (no repeats).
        /// Server only.
        /// </summary>
        public void SpawnAll()
        {
            if (!IsServer) return;

            // Work on a mutable copy so the original list isn't mutated
            // (allows replaying in tests or re-use across rounds).
            var availablePoints = new List<Transform>(spawnPoints);

            foreach (var (clientId, prefabHash) in _playerChoices)
            {
                Vector3 spawnPosition = PickSpawnPoint(availablePoints);
                SpawnForClient(clientId, prefabHash, spawnPosition);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        private static Vector3 PickSpawnPoint(List<Transform> points)
        {
            int index = Random.Range(0, points.Count);
            Vector3 position = points[index].position;
            points.RemoveAt(index);
            return position;
        }

        private void SpawnForClient(ulong clientId, ulong prefabHash, Vector3 position)
        {
            foreach (NetworkPrefab netPrefab in NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists[0].PrefabList)
            {
                if (netPrefab.SourcePrefabGlobalObjectIdHash != prefabHash) continue;

                GameObject go = Instantiate(netPrefab.Prefab, position, Quaternion.identity);
                go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
                return;
            }

            Debug.LogWarning($"[CharacterSpawner] No prefab found for hash {prefabHash} (client {clientId}).");
        }
    }
}
