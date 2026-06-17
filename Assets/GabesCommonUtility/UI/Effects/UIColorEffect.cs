using System;
using UI.Effects.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Effects
{
    /// <summary>Tints a Graphic along a gradient (or toward white) when active, optional pulse.</summary>
    [Serializable]
    public class UIColorEffect : UIEffect
    {
        [SerializeField] private Graphic graphic;
        [SerializeField] private ColorAnimationDataSo animationData;

        private Color _originalColor;
        private Gradient _runtimeGradient;

        protected override float Duration => animationData ? animationData.TransitionDuration : 0f;
        protected override AnimationCurve Curve => animationData ? animationData.TransitionCurve : null;
        protected override bool SustainWhileActive => animationData && animationData.Pulse.Enabled;

        protected override void OnInit()
        {
            if (!graphic) graphic = Owner.GetComponent<Graphic>();
            if (!animationData) animationData = AnimationEffectDefaults.Load<ColorAnimationDataSo>(AnimationEffectDefaults.ColorKey);

            if (!graphic || !animationData) return;

            _originalColor = graphic.color;

            if (animationData.ColorGradient != null)
            {
                _runtimeGradient = new Gradient();
                _runtimeGradient.SetKeys(animationData.ColorGradient.colorKeys, animationData.ColorGradient.alphaKeys);

                if (animationData.UseOriginalColorAsStart)
                {
                    var keys = _runtimeGradient.colorKeys;
                    if (keys.Length > 0)
                    {
                        keys[0] = new GradientColorKey(_originalColor, 0f);
                        _runtimeGradient.SetKeys(keys, _runtimeGradient.alphaKeys);
                    }
                }
            }
        }

        protected override void Apply(float blend)
        {
            if (!graphic || !animationData) return;

            float t = Mathf.Clamp01(blend + animationData.Pulse.Evaluate(OscClock, blend));
            graphic.color = _runtimeGradient?.Evaluate(t) ?? Color.LerpUnclamped(_originalColor, Color.white, t);
        }
    }
}
