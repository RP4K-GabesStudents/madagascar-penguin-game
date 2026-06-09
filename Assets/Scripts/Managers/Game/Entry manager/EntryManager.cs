using System;
using System.Collections;
using System.Collections.Generic;
using Eflatun.SceneReference;
using Game.Characters;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers.Game
{
    /// <summary>
    /// Orchestrates the entry/lobby sequence: scene loading, character selection,
    /// spawning, and kicking off the game. Delegates UI, timing, spawning,
    /// and door logic to focused sub-components.
    /// </summary>
    [RequireComponent(typeof(CharacterSpawner))]
    [RequireComponent(typeof(SelectionTimer))]
    [RequireComponent(typeof(DoorController))]
    [RequireComponent(typeof(EntryUIController))]
    public class EntryManager : NetworkBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private SceneReference selectionScene;

        [Header("Settings")]
        [SerializeField] private float selectionTime = 22f;

        // Sub-components
        private CharacterSpawner _spawner;
        private SelectionTimer _selectionTimer;
        private DoorController _doorController;
        private EntryUIController _uiController;

        private bool _isSelectionSpawn;

        // ─────────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _spawner        = GetComponent<CharacterSpawner>();
            _selectionTimer = GetComponent<SelectionTimer>();
            _doorController = GetComponent<DoorController>();
            _uiController   = GetComponent<EntryUIController>();
        }

        private void OnEnable()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += OnAllClientsLoaded;
            NetworkManager.SceneManager.OnLoadComplete       += OnLocalLoaded;
        }

        private void OnDisable()
        {
            if (!NetworkManager || NetworkManager.SceneManager == null) return;
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnAllClientsLoaded;
            NetworkManager.SceneManager.OnLoadComplete       -= OnLocalLoaded;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Scene load callbacks
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Called on each client once the entry scene finishes loading locally.
        /// Loads the additive selection scene and hooks up character-selected events.
        /// </summary>
        private async void OnLocalLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (_isSelectionSpawn) return;
            _isSelectionSpawn = true;

            try
            {
                await SceneManager.LoadSceneAsync(selectionScene.BuildIndex, LoadSceneMode.Additive);
                SelectionManager.Instance.OnCharacterSelected += RequestSpawnCharacter;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EntryManager] Failed while loading local client: {e.Message}");
            }
        }

        /// <summary>
        /// Called on the server once every client has completed loading.
        /// Starts the selection timer and triggers the explosion animation on clients.
        /// </summary>
        private void OnAllClientsLoaded(
            string sceneName, LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (IsClient)
                _uiController.PlayExplosionEntry();

            if (!IsServer) return;

            _selectionTimer.StartTimer(selectionTime, OnTimerExpired);
            _doorController.SetDoorsOpen(false);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Character selection → spawn flow
        // ─────────────────────────────────────────────────────────────────────

        private void RequestSpawnCharacter(GenericCharacter character)
        {
            ulong prefabHash = character.GetComponent<NetworkObject>().PrefabIdHash;
            ChoosePenguin_ServerRpc(prefabHash);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChoosePenguin_ServerRpc(ulong prefabHash, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            _spawner.RegisterChoice(senderId, prefabHash);

            bool allPlayersChosen = _spawner.ChoiceCount == NetworkManager.ConnectedClients.Count;
            if (!allPlayersChosen) return;

            // Everyone has chosen — stop the timer, spawn, and start the game.
            _selectionTimer.StopTimer();
            _spawner.SpawnAll();
            OnGameStarting_ClientRpc();
            _doorController.ScheduleOpen();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Timer callback
        // ─────────────────────────────────────────────────────────────────────

        private void OnTimerExpired()
        {
            ForceChoosePenguin_ClientRpc();
        }

        [ClientRpc]
        private void ForceChoosePenguin_ClientRpc()
        {
            SelectionManager.Instance.SelectCurPenguin();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Game starting
        // ─────────────────────────────────────────────────────────────────────

        [ClientRpc]
        private void OnGameStarting_ClientRpc()
        {
            SceneManager.UnloadSceneAsync(selectionScene.BuildIndex);
            _uiController.OnGameStarting();
        }
    }
}
