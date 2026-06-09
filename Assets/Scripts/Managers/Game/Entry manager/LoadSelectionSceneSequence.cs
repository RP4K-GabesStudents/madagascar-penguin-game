#if UNITASK

using System;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using GabesCommonUtility.Sequence;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers.Game
{
    /// <summary>
    /// Sequence Step 1 of 4.
    ///
    /// Responsibilities (runs on every client independently):
    ///   - Loads the character-selection scene additively.
    ///   - Triggers the explosion animator + overlay on all clients via ClientRpc.
    ///   - On the server: initialises and syncs the countdown NetworkVariable.
    ///
    /// Completes immediately after the scene is loaded and the RPC fires,
    /// then hands off to <see cref="CharacterSelectionSequence"/>.
    /// </summary>
    public class LoadSelectionSceneSequence : NetworkBehaviour, IEntrySequence
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Scene")]
        [SerializeField] private SceneReference selectionScene;

        [Header("Next Step")]
        [SerializeField] private CharacterSelectionSequence next;

        [Header("Explosion UI")]
        [SerializeField] private Animator sceneAnimator;
        [SerializeField] private Image    explosionOverlay;

        [Header("Timer UI")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Selection Settings")]
        [SerializeField] private float selectionTime = 22f;

        // ── Animator hash ────────────────────────────────────────────────────
        private static readonly int OnExplodedHash = Animator.StringToHash("OnExploded");

        // ── IEntrySequence ───────────────────────────────────────────────────
        public event Action<string> DisplayMessage;
        public IEntrySequence Default  => next;
        public bool           IsCompleted { get; private set; }

        // ── NetworkVariable (server writes, clients read) ─────────────────────
        private readonly NetworkVariable<float> _timeRemaining = new();

        // ─────────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (explosionOverlay) explosionOverlay.enabled = false;

            // Mirror the synced time value into the timer text on every client.
            _timeRemaining.OnValueChanged += (_, newValue) =>
            {
                if (timerText) timerText.SetText(newValue.ToString("N0"));
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // IEntrySequence
        // ─────────────────────────────────────────────────────────────────────

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            DisplayMessage?.Invoke("[LoadSelectionSceneSequence] Loading selection scene…");

            // Every client loads the selection scene additively.
            await SceneManager.LoadSceneAsync(selectionScene.BuildIndex, LoadSceneMode.Additive);

            DisplayMessage?.Invoke("[LoadSelectionSceneSequence] Selection scene loaded.");

            // Trigger explosion visuals on every client and start the server timer.
            PlayExplosionAndStartTimer_ClientRpc();

            // Pass the shared timer NetworkVariable reference to the next step
            // so it can display / manipulate the same value.
            next.Initialise(selectionScene, selectionTime, _timeRemaining);

            IsCompleted = true;
            return Default;
        }

        // ─────────────────────────────────────────────────────────────────────
        // RPCs
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fires on all clients: plays the explosion animator and shows the overlay.
        /// On the server side also initialises the synced countdown timer.
        /// </summary>
        [ClientRpc]
        private void PlayExplosionAndStartTimer_ClientRpc()
        {
            if (sceneAnimator)   sceneAnimator.SetTrigger(OnExplodedHash);
            if (explosionOverlay) explosionOverlay.enabled = true;

            // Only the server drives the timer value.
            if (!IsServer) return;
            _timeRemaining.Value = selectionTime;
        }
    }
}

#endif
