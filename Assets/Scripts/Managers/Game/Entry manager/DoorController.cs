using System.Collections;
using UnityEngine;

namespace Managers.Game
{
    /// <summary>
    /// Controls the door animators: immediately opening/closing them
    /// or scheduling a delayed open after all players have spawned.
    /// </summary>
    public class DoorController : MonoBehaviour
    {
        private static readonly int IsOpenHash = Animator.StringToHash("isOpen");

        [Header("Door Animators")]
        [SerializeField] private Animator[] doors;

        [Header("Settings")]
        [SerializeField] private float doorOpenDelay = 5f;

        private Coroutine _delayedOpenCoroutine;

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Immediately sets every door to open or closed.</summary>
        public void SetDoorsOpen(bool open)
        {
            foreach (Animator door in doors)
                door.SetBool(IsOpenHash, open);
        }

        /// <summary>
        /// Opens all doors after <see cref="doorOpenDelay"/> seconds.
        /// Any previously scheduled open is cancelled first.
        /// </summary>
        public void ScheduleOpen()
        {
            if (_delayedOpenCoroutine != null)
                StopCoroutine(_delayedOpenCoroutine);

            _delayedOpenCoroutine = StartCoroutine(DelayedOpenRoutine());
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator DelayedOpenRoutine()
        {
            yield return new WaitForSeconds(doorOpenDelay);
            SetDoorsOpen(true);
            _delayedOpenCoroutine = null;
        }
    }
}
