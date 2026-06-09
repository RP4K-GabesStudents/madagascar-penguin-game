#if UNITASK

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using GabesCommonUtility.Sequence;
using Unity.Netcode;
using UnityEngine;
using NetworkPrefab = Unity.Netcode.NetworkPrefab;

namespace Managers.Game
{
    /// <summary>
    /// Sequence Step 3 of 4.
    ///
    /// Responsibilities (server-authoritative):
    ///   - Spawns the correct networked prefab for every player at a random spawn point.
    ///   - Fires a ClientRpc so all clients know spawning is done and can transition UI.
    ///
    /// Call <see cref="Initialise"/> before <see cref="ExecuteSequence"/> (done by
    /// <see cref="CharacterSelectionSequence"/>).
    /// </summary>
    public class SpawnCharactersSequence : NetworkBehaviour, IEntrySequence
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Spawn Points")]
        [SerializeField] private List<Transform> spawnPoints;

        [Header("Next Step")]
        [SerializeField] private GameStartSequence next;

        // ── IEntrySequence ───────────────────────────────────────────────────
        public event Action<string> DisplayMessage;
        public IEntrySequence Default     => next;
        public bool           IsCompleted { get; private set; }

        // ── State (injected by previous step) ────────────────────────────────
        private SceneReference              _selectionScene;
        private Dictionary<ulong, ulong>    _playerChoices;

        // ── Completion signal ─────────────────────────────────────────────────
        private UniTaskCompletionSource _spawnsDone;

        // ─────────────────────────────────────────────────────────────────────
        // Setup
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Injects the player choices collected by <see cref="CharacterSelectionSequence"/>.
        /// Must be called before <see cref="ExecuteSequence"/>.
        /// </summary>
        public void Initialise(SceneReference selectionScene, Dictionary<ulong, ulong> playerChoices)
        {
            _selectionScene = selectionScene;
            _playerChoices  = playerChoices;
        }

        // ─────────────────────────────────────────────────────────────────────
        // IEntrySequence
        // ─────────────────────────────────────────────────────────────────────

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            DisplayMessage?.Invoke("[SpawnCharactersSequence] Spawning characters…");

            _spawnsDone = new UniTaskCompletionSource();

            if (IsServer)
            {
                SpawnAll();
                // Notify every client (including the server-client) that spawning finished.
                NotifySpawnsDone_ClientRpc();
            }

            // All clients wait here until the server fires the RPC.
            await _spawnsDone.Task;

            // Pass the scene reference forward so the final step can unload it.
            next.Initialise(_selectionScene);

            IsCompleted = true;
            return Default;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Spawning (server only)
        // ─────────────────────────────────────────────────────────────────────

        private void SpawnAll()
        {
            var availablePoints = new List<Transform>(spawnPoints);

            foreach (var (clientId, prefabHash) in _playerChoices)
            {
                Vector3 position = PickSpawnPoint(availablePoints);
                SpawnForClient(clientId, prefabHash, position);
            }

            DisplayMessage?.Invoke($"[SpawnCharactersSequence] Spawned {_playerChoices.Count} character(s).");
        }

        private static Vector3 PickSpawnPoint(List<Transform> points)
        {
            int index    = Random.Range(0, points.Count);
            Vector3 pos  = points[index].position;
            points.RemoveAt(index);
            return pos;
        }

        private void SpawnForClient(ulong clientId, ulong prefabHash, Vector3 position)
        {
            foreach (NetworkPrefab netPrefab in NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists[0].PrefabList)
            {
                if (netPrefab.SourcePrefabGlobalObjectIdHash != prefabHash) continue;

                GameObject go = Instantiate(netPrefab.Prefab, position, Quaternion.identity);
                go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

                DisplayMessage?.Invoke($"[SpawnCharactersSequence] Spawned prefab {prefabHash} for client {clientId}.");
                return;
            }

            Debug.LogWarning($"[SpawnCharactersSequence] No prefab found for hash {prefabHash} (client {clientId}).");
        }

        // ─────────────────────────────────────────────────────────────────────
        // RPC
        // ─────────────────────────────────────────────────────────────────────

        [ClientRpc]
        private void NotifySpawnsDone_ClientRpc()
        {
            _spawnsDone?.TrySetResult();
        }
    }
}

#endif
