using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Managers.Game
{
    /// <summary>
    /// Owns all client-side UI for the entry sequence:
    /// the explosion overlay, mission-text crawl, and any panels
    /// that are hidden until the game starts.
    /// </summary>
    public class EntryUIController : MonoBehaviour
    {
        private static readonly int OnExplodedHash = Animator.StringToHash("OnExploded");

        [Header("Explosion")]
        [SerializeField] private Animator sceneAnimator;
        [SerializeField] private Image explosionOverlay;

        [Header("Mission Text")]
        [SerializeField] private TextMeshProUGUI missionText;
        [SerializeField] private SceneAsset levelScene;
        [SerializeField] private float textSpeed    = 0.05f;
        [SerializeField] private float textDuration = 3f;

        [Header("Late-Enable Objects")]
        [SerializeField] private GameObject[] enableOnGameStart;

        [Header("Timer UI (reference for hiding)")]
        [SerializeField] private SelectionTimer selectionTimer;

        // ─────────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            explosionOverlay.enabled = false;

            foreach (GameObject go in enableOnGameStart)
                go.SetActive(false);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Triggers the entry explosion animation and shows the overlay.
        /// Called on every client when all clients have loaded the scene.
        /// </summary>
        public void PlayExplosionEntry()
        {
            sceneAnimator.SetTrigger(OnExplodedHash);
            explosionOverlay.enabled = true;
        }

        /// <summary>
        /// Transitions the UI from the selection phase into the game:
        /// hides the explosion overlay, hides the timer, activates hidden panels,
        /// and begins the mission-text crawl.
        /// </summary>
        public void OnGameStarting()
        {
            explosionOverlay.enabled = false;
            selectionTimer.HideUI();

            foreach (GameObject go in enableOnGameStart)
                go.SetActive(true);

            StartCoroutine(MissionTextCrawlRoutine());
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator MissionTextCrawlRoutine()
        {
            missionText.enabled = true;
            missionText.text    = string.Empty;

            string fullText = BuildMissionText();

            foreach (char c in fullText)
            {
                missionText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }

            yield return new WaitForSeconds(textDuration);
            missionText.enabled = false;
        }

        private string BuildMissionText()
        {
            return $"Level: {levelScene.name}\n" +
                   $"Time: {DateTime.Now:HH:mm:ss}\n" +
                   "Mission: Defeat Horses";
        }
    }
}
