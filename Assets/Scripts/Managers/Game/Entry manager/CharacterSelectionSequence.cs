#if UNITASK

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Game.Characters;
using GabesCommonUtility.Sequence;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Managers.Game
{
    /// <summary>
    /// Sequence Step 2 of 4.
    ///
    /// Responsibilities:
    ///   - Subscribes to <see cref="SelectionManager.OnCharacterSelected"/> on each client.
    ///   - Forwards the local player's choice to the server via <see cref="ChoosePenguin_ServerRpc"/>.
    ///   - On the server: ticks the countdown; when time runs out forces every client
    ///     to commit their current selection via <see cref="ForceChoosePenguin_ClientRpc"/>.
    ///   - Completes once every connected player has submitted a choice.
    ///
    /// Call <see cref="Initialise"/> before <see cref="ExecuteSequence"/> (done by
    /// <see cref="LoadSelectionSceneSequence"/>).
    /// </summary>
    public class CharacterSelectionSequence : NetworkBehaviour, IEntrySequence
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Next Step")]
        [SerializeField] private SpawnCharactersSequence next;

        [Header("Timer UI")]
        [SerializeField] private TextMeshProUGUI timerText;

        // ── IEntrySequence ───────────────────────────────────────────────────
        public event Action<string> DisplayMessage;
        public IEntrySequence Default     => next;
        public bool           IsCompleted { get; private set; }

        // ── State (injected by previous step) ────────────────────────────────
        private SceneReference                _selectionScene;
        private float                         _selectionTime;
        private NetworkVariable<float>        _timeRemaining;

        // ── Internal ─────────────────────────────────────────────────────────
        private readonly Dictionary<ulong, ulong> _playerChoices = new();
        private UniTaskCompletionSource           _allChosen;
        private bool                              _timerExpired;

        // ─────────────────────────────────────────────────────────────────────
        // Setup (called by previous sequence step before ExecuteSequence)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Injects dependencies from <see cref="LoadSelectionSceneSequence"/>.
        /// Must be called before <see cref="ExecuteSequence"/>.
        /// </summary>
        public void Initialise(
            SceneReference         selectionScene,
            float                  selectionTime,
            NetworkVariable<float> timeRemaining)
        {
            _selectionScene = selectionScene;
            _selectionTime  = selectionTime;
            _timeRemaining  = timeRemaining;
        }

        // ─────────────────────────────────────────────────────────────────────
        // IEntrySequence
        // ─────────────────────────────────────────────────────────────────────

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            DisplayMessage?.Invoke("[CharacterSelectionSequence] Waiting for all players to choose…");

            _allChosen = new UniTaskCompletionSource();
            _playerChoices.Clear();

            // Every client hooks up the local selection event.
            SelectionManager.Instance.OnCharacterSelected += OnLocalCharacterSelected;

            // The server also runs the countdown concurrently.
            if (IsServer)
                RunCountdown().Forget();

            // Await until the server signals all players have chosen.
            await _allChosen.Task;

            SelectionManager.Instance.OnCharacterSelected -= OnLocalCharacterSelected;

            // Pass collected choices to the spawner step.
            next.Initialise(_selectionScene, _playerChoices);

            IsCompleted = true;
            return Default;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Local selection → server RPC
        // ─────────────────────────────────────────────────────────────────────

        private void OnLocalCharacterSelected(GenericCharacter character)
        {
            ulong prefabHash = character.GetComponent<NetworkObject>().PrefabIdHash;
            ChoosePenguin_ServerRpc(prefabHash);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChoosePenguin_ServerRpc(ulong prefabHash, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            // Record or overwrite this client's choice.
            if (!_playerChoices.TryAdd(senderId, prefabHash))
                _playerChoices[senderId] = prefabHash;

            DisplayMessage?.Invoke($"[CharacterSelectionSequence] Player {senderId} chose prefab {prefabHash}. " +
                                   $"({_playerChoices.Count}/{NetworkManager.ConnectedClients.Count})");

            if (_playerChoices.Count < NetworkManager.ConnectedClients.Count) return;

            // All players have chosen — complete.
            CompleteSelection();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Countdown (server only)
        // ─────────────────────────────────────────────────────────────────────

        private async UniTaskVoid RunCountdown()
        {
            while (_timeRemaining.Value > 0f && !_timerExpired)
            {
                _timeRemaining.Value -= Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (_timerExpired) return; // Already resolved via all-chosen path.

            DisplayMessage?.Invoke("[CharacterSelectionSequence] Timer expired — forcing selection.");
            ForceChoosePenguin_ClientRpc();
        }

        [ClientRpc]
        private void ForceChoosePenguin_ClientRpc()
        {
            SelectionManager.Instance.SelectCurPenguin();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Completion
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Called on the server when all choices are in (timer or voluntary).
        /// Notifies all clients so their awaiting tasks also resolve.
        /// </summary>
        private void CompleteSelection()
        {
            _timerExpired = true;
            NotifySelectionComplete_ClientRpc();
        }

        [ClientRpc]
        private void NotifySelectionComplete_ClientRpc()
        {
            // Resolve the UniTask on every client (including the host/server client).
            _allChosen?.TrySetResult();
        }
    }
}

#endif
