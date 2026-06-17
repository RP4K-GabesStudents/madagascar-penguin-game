using System;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Effects
{
    /// <summary>Fires UnityEvents on the state flip instead of animating. Settles instantly.</summary>
    [Serializable]
    public class UIActionEffect : UIEffect
    {
        [SerializeField] private UnityEvent onActivate;
        [SerializeField] private UnityEvent onDeactivate;

        private static readonly AnimationCurve Linear = AnimationCurve.Linear(0, 0, 1, 1);

        protected override float Duration => 0f;
        protected override AnimationCurve Curve => Linear;
        protected override void Apply(float blend) { }

        protected override void OnStateChanged(bool active)
        {
            if (active) onActivate?.Invoke();
            else onDeactivate?.Invoke();
        }
    }
}
