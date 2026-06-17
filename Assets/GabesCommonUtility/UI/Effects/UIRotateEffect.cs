using System;
using UI.Effects.ScriptableObjects;
using UnityEngine;

namespace UI.Effects
{
    /// <summary>Held tilt plus optional oscillating swing around an axis.</summary>
    [Serializable]
    public class UIRotateEffect : UIEffect
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector3 axis = Vector3.forward;
        [SerializeField] private RotateAnimationDataSo animationData;

        private Quaternion _original;

        protected override float Duration => animationData ? animationData.TransitionDuration : 0f;
        protected override AnimationCurve Curve => animationData ? animationData.TransitionCurve : null;
        protected override bool SustainWhileActive => animationData && animationData.Swing.Enabled;

        protected override void OnInit()
        {
            if (!target) target = Owner.transform as RectTransform;
            if (!animationData) animationData = AnimationEffectDefaults.Load<RotateAnimationDataSo>(AnimationEffectDefaults.RotateKey);

            if (!target || !animationData) return;

            _original = target.localRotation;
        }

        protected override void Apply(float blend)
        {
            if (!target || !animationData) return;

            // Absolute mode targets the SO angle directly; relative adds it to rest.
            float held = animationData.RotationZ * blend;
            float angle = held + animationData.Swing.Evaluate(OscClock, blend);

            Quaternion baseRot = animationData.UseAbsoluteRotation
                ? Quaternion.identity
                : _original;
            target.localRotation = baseRot * Quaternion.AngleAxis(angle, axis);
        }
    }
}
