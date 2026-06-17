using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace UI.Effects.ScriptableObjects
{
    [MovedFrom(true, sourceClassName: "AnimationDataSo")]
    [CreateAssetMenu(fileName = "ScaleAnimationData", menuName = "GabesCommonUtility/UI/Scale Animation Data")]
    public class ScaleAnimationDataSo : AnimationDataSoBase
    {
        [SerializeField] private Vector2 hoverSize = Vector2.one;
        [SerializeField] private bool useLiteralScale = true;
        [Tooltip("Optional continuous pulse layered on top of the held scale. Range units match the scale (literal or sizeDelta).")]
        [SerializeField] private UIOscillation pulse;

        public Vector2 HoverSize => hoverSize;
        public bool UseLiteralScale => useLiteralScale;
        public UIOscillation Pulse => pulse;

        // Default overshoot curve preserved from the original AnimationDataSo.
        private void Reset()
        {
            transitionCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(0.1f, -0.2f, 0f, 2f),
                new Keyframe(0.9f, 1.2f, 2f, 0f),
                new Keyframe(1f, 1f, 0f, 0f)
            );
        }
    }
}
