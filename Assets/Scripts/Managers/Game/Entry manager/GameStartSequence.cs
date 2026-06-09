#if UNITASK

using System;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using GabesCommonUtility.Sequence;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers.Game
{
    /// <summary>
    /// Sequence Step 4 of 4  (terminal — <see cref="Default"/> is null).
    ///
    /// Responsibilities (all client-local; no networking needed at this stage):
    ///   - Hides the explosion overlay and timer UI.
    ///   - Activates objects that were hidden during the lobby phase.
    ///   - Unloads the additive character-selection scene.
    ///   - Runs the typewriter-style mission-text crawl.
    ///   - Opens the doors after a configurable delay.
    ///
    /// Call <see cref="Initialise"/> before <see cref="ExecuteSequence"/> (done by
    /// <see cref="SpawnCharactersSequence"/>).
    /// </summary>
    public class GameStartSequence : MonoBehaviour, IEntrySequence
    {
        // ── Animator hash ────────────────────────────────────────────────────
        private static readonly int IsOpenHash = Animator.StringToHash("isOpen");

        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Explosion / Timer UI to hide")]
        [SerializeField] private Image              explosionOverlay;
        [SerializeField] private GameObject         timerObject;

        [Header("Mission Text")]
        [SerializeField] private TextMeshProUGUI    missionText;
        [SerializeField] private SceneAsset         levelScene;
        [SerializeField] private float              textSpeed    = 0.05f;
        [SerializeField] private float              textDuration = 3f;

        [Header("Late-Enable Objects")]
        [SerializeField] private GameObject[]       enableOnGameStart;

        [Header("Doors")]
        [SerializeField] private Animator[]         doors;
        [SerializeField] private float              doorOpenDelay = 5f;

        // ── IEntrySequence ───────────────────────────────────────────────────
        /// <summary>
        /// Terminal step — returns null to stop the <see cref="SequenceEntryPoint"/> chain.
        /// </summary>
        public event Action<string> DisplayMessage;
        public IEntrySequence Default     => null;
        public bool           IsCompleted { get; private set; }

        // ── State ─────────────────────────────────────────────────────────────
        private SceneReference _selectionScene;

        // ─────────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (explosionOverlay) explosionOverlay.enabled = false;

            foreach (GameObject go in enableOnGameStart)
                go.SetActive(false);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Setup
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Injects the selection scene reference so it can be unloaded.
        /// Must be called before <see cref="ExecuteSequence"/>.
        /// </summary>
        public void Initialise(SceneReference selectionScene)
        {
            _selectionScene = selectionScene;
        }

        // ─────────────────────────────────────────────────────────────────────
        // IEntrySequence
        // ─────────────────────────────────────────────────────────────────────

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            DisplayMessage?.Invoke("[GameStartSequence] Game starting — preparing UI…");

            // ── 1. Hide lobby UI ─────────────────────────────────────────────
            if (explosionOverlay) explosionOverlay.enabled = false;
            if (timerObject)      timerObject.SetActive(false);

            // ── 2. Activate gameplay objects ─────────────────────────────────
            foreach (GameObject go in enableOnGameStart)
                go.SetActive(true);

            // ── 3. Unload selection scene ─────────────────────────────────────
            if (_selectionScene != null)
            {
                DisplayMessage?.Invoke("[GameStartSequence] Unloading selection scene…");
                await SceneManager.UnloadSceneAsync(_selectionScene.BuildIndex);
            }

            // ── 4. Mission text crawl ─────────────────────────────────────────
            DisplayMessage?.Invoke("[GameStartSequence] Playing mission text…");
            await PlayMissionTextAsync();

            // ── 5. Open doors after delay (fire-and-forget; don't block chain) ─
            OpenDoorsAfterDelay().Forget();

            DisplayMessage?.Invoke("[GameStartSequence] Entry sequence complete.");
            IsCompleted = true;

            // Terminal step — return null to end the SequenceEntryPoint chain.
            return Default;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        private async UniTask PlayMissionTextAsync()
        {
            if (!missionText) return;

            missionText.enabled = true;
            missionText.text    = string.Empty;

            string fullText = BuildMissionText();

            foreach (char c in fullText)
            {
                missionText.text += c;
                await UniTask.WaitForSeconds(textSpeed);
            }

            await UniTask.WaitForSeconds(textDuration);
            missionText.enabled = false;
        }

        private string BuildMissionText() =>
            $"Level: {(levelScene ? levelScene.name : "Unknown")}\n" +
            $"Time: {DateTime.Now:HH:mm:ss}\n" +
            "Mission: Defeat Horses";

        private async UniTaskVoid OpenDoorsAfterDelay()
        {
            await UniTask.WaitForSeconds(doorOpenDelay);

            foreach (Animator door in doors)
                door.SetBool(IsOpenHash, true);

            DisplayMessage?.Invoke("[GameStartSequence] Doors opened.");
        }
    }
}

#endif
