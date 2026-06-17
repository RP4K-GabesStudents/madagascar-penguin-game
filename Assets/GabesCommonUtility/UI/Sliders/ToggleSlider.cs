using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITASK
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
#else
using System.Collections;
#endif

namespace UI.Sliders
{
    public class ToggleSlider : Slider
    {
        [SerializeField, Tooltip("Current toggle state.")]
        private bool state;

        [Header("Animation")]
        [SerializeField, Tooltip("Animate the value change when toggled. If off, the value snaps instantly.")]
        private bool animate = true;

        [SerializeField, Tooltip("Seconds the toggle animation takes.")]
        private float duration = 0.2f;

        [SerializeField, Tooltip("Curve evaluated over normalized time 0..1. Its output 0..1 maps from the start value to the target value, so overshoot/ease-out curves are fine.")]
        private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField, Tooltip("If true, onValueChanged fires every frame during the animation instead of once at the start of the toggle.")]
        private bool fireValueChangedDuringAnimation = false;

#if UNITASK
        private CancellationTokenSource _animCts;
#else
        private Coroutine _animRoutine;
#endif

        public bool State
        {
            get => state;
            set
            {
                if (state == value) return;
                state = value;
                UpdateVisualState();

                if (!fireValueChangedDuringAnimation)
                    onValueChanged.Invoke(state ? maxValue : minValue);
            }
        }

        protected override void Start()
        {
            base.Start();
            // Snap on start (no animation on initial layout)
            SnapToState();
        }

        private void UpdateVisualState()
        {
            // Animate at runtime if enabled and the object is active; otherwise snap.
            if (animate && Application.isPlaying && isActiveAndEnabled && duration > 0f)
                StartAnimation();
            else
                SnapToState();
        }

        private void SnapToState()
        {
            StopAnimation();
            value = state ? maxValue : minValue;
        }

#if UNITASK
        private void StartAnimation()
        {
            StopAnimation();
            _animCts = new CancellationTokenSource();
            AnimateValue(_animCts.Token).Forget();
        }

        private void StopAnimation()
        {
            if (_animCts != null)
            {
                _animCts.Cancel();
                _animCts.Dispose();
                _animCts = null;
            }
        }

        private async UniTaskVoid AnimateValue(CancellationToken token)
        {
            float from = value;                          // start from wherever we currently are
            float target = state ? maxValue : minValue;  // 0->1 or 1->0 (relative to min/max)

            float elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float eased = curve.Evaluate(t);

                    value = Mathf.LerpUnclamped(from, target, eased);

                    if (fireValueChangedDuringAnimation)
                        onValueChanged.Invoke(value);

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                value = target;
                if (fireValueChangedDuringAnimation)
                    onValueChanged.Invoke(value);
            }
            catch (OperationCanceledException)
            {
                // Toggled again or disabled mid-animation — leave value where it is so the
                // next animation reverses smoothly from the current position.
            }
        }
#else
        private void StartAnimation()
        {
            StopAnimation();
            _animRoutine = StartCoroutine(AnimateValue());
        }

        private void StopAnimation()
        {
            if (_animRoutine != null)
            {
                StopCoroutine(_animRoutine);
                _animRoutine = null;
            }
        }

        private IEnumerator AnimateValue()
        {
            float from = value;                          // start from wherever we currently are
            float target = state ? maxValue : minValue;  // 0->1 or 1->0 (relative to min/max)

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = curve.Evaluate(t);

                value = Mathf.LerpUnclamped(from, target, eased);

                if (fireValueChangedDuringAnimation)
                    onValueChanged.Invoke(value);

                yield return null;
            }

            value = target;
            if (fireValueChangedDuringAnimation)
                onValueChanged.Invoke(value);

            _animRoutine = null;
        }
#endif

        public override void OnPointerDown(PointerEventData eventData)
        {
            // Toggle instead of dragging
            State = !State;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            // Disable dragging behavior entirely
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // Make sure we don't leave a dangling animation and end in a consistent state.
            SnapToState();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (duration < 0f) duration = 0f;
            // Editor-time always snaps (async/coroutines don't run outside play mode).
            value = state ? maxValue : minValue;
        }
#endif
    }
}