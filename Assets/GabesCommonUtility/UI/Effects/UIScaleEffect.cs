using System;
using UI.Effects.ScriptableObjects;
using UnityEngine;

namespace UI.Effects
{
    /// <summary>Scales toward animationData.HoverSize when active, optional pulse.</summary>
    [Serializable]
    public class UIScaleEffect : UIEffect
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private ScaleAnimationDataSo animationData;

        private Vector2 _original;

        protected override float Duration => animationData ? animationData.TransitionDuration : 0f;
        protected override AnimationCurve Curve => animationData ? animationData.TransitionCurve : null;
        protected override bool SustainWhileActive => animationData && animationData.Pulse.Enabled;

        protected override void OnInit()
        {
            if (!target) target = Owner.transform as RectTransform;
            if (!animationData) animationData = AnimationEffectDefaults.Load<ScaleAnimationDataSo>(AnimationEffectDefaults.ScaleKey);

            if (!target || !animationData) return; // inert: Apply guards on the same conditions

            _original = animationData.UseLiteralScale ? (Vector2)target.localScale : target.sizeDelta;
        }

        protected override void Apply(float blend)
        {
            if (!target || !animationData) return;

            Vector2 value = Vector2.LerpUnclamped(_original, animationData.HoverSize, blend)
                            + Vector2.one * animationData.Pulse.Evaluate(OscClock, blend);

            if (animationData.UseLiteralScale)
                target.localScale = new Vector3(value.x, value.y, target.localScale.z);
            else
                target.sizeDelta = value;
        }
    }
}
