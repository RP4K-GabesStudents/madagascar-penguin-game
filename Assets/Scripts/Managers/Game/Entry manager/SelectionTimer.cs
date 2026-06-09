using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Managers.Game
{
    /// <summary>
    /// Manages the character-selection countdown timer.
    /// The server drives the <see cref="NetworkVariable{T}"/> so all clients
    /// see the same value; the UI text is updated locally via the change callback.
    /// </summary>
    public class SelectionTimer : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI timerText;

        // Synced timer value; clients read-only, server writes.
        private readonly NetworkVariable<float> _timeRemaining = new();

        private Coroutine _timerCoroutine;
        private Action _onExpired;

        // ─────────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _timeRemaining.OnValueChanged += OnTimeChanged;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Starts the countdown from <paramref name="duration"/> seconds.
        /// <paramref name="onExpired"/> is invoked on the server when time reaches zero.
        /// Only has effect when called on the server.
        /// </summary>
        public void StartTimer(float duration, Action onExpired)
        {
            if (!IsServer) return;

            _onExpired = onExpired;
            _timeRemaining.Value = duration;

            if (_timerCoroutine != null)
                StopCoroutine(_timerCoroutine);

            _timerCoroutine = StartCoroutine(CountdownRoutine());
        }

        /// <summary>Stops the running countdown. Server only.</summary>
        public void StopTimer()
        {
            if (!IsServer) return;

            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
        }

        /// <summary>Hides the timer UI element.</summary>
        public void HideUI() => timerText.gameObject.SetActive(false);

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator CountdownRoutine()
        {
            while (_timeRemaining.Value > 0f)
            {
                _timeRemaining.Value -= Time.deltaTime;
                yield return null;
            }

            _timerCoroutine = null;
            _onExpired?.Invoke();
        }

        private void OnTimeChanged(float oldValue, float newValue)
        {
            timerText.SetText(newValue.ToString("N0"));
        }
    }
}
