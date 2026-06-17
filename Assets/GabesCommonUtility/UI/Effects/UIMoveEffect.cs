using System;
using UI.Effects.ScriptableObjects;
using UnityEngine;

namespace UI.Effects
{
    /// <summary>Offsets anchoredPosition when active, optional bob.</summary>
    [Serializable]
    public class UIMoveEffect : UIEffect
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private MoveAnimationDataSo animationData;

        private Vector2 _original;
        private Vector2 _activeTarget;

        protected override float Duration => animationData ? animationData.TransitionDuration : 0f;
        protected override AnimationCurve Curve => animationData ? animationData.TransitionCurve : null;
        protected override bool SustainWhileActive => animationData && animationData.Bob.Enabled;

        protected override void OnInit()
        {
            if (!target) target = Owner.transform as RectTransform;
            if (!animationData) animationData = AnimationEffectDefaults.Load<MoveAnimationDataSo>(AnimationEffectDefaults.MoveKey);

            if (!target || !animationData) return;

            _original = target.anchoredPosition;
            _activeTarget = animationData.UseAbsolutePosition
                ? animationData.MoveOffset
                : _original + animationData.MoveOffset;
        }

        protected override void Apply(float blend)
        {
            if (!target || !animationData) return;

            target.anchoredPosition = Vector2.LerpUnclamped(_original, _activeTarget, blend)
                                      + animationData.BobAxis.normalized * animationData.Bob.Evaluate(OscClock, blend);
        }
    }
}
