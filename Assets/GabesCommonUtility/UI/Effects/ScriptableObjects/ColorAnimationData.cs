using UnityEngine;

namespace UI.Effects.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ColorAnimationData", menuName = "GabesCommonUtility/UI/Color Animation Data")]
    public class ColorAnimationDataSo : AnimationDataSoBase
    {
        [SerializeField] private Gradient colorGradient = new();
        [SerializeField] private bool useOriginalColorAsStart = true;
        [Tooltip("Optional continuous pulse layered on top of the blend, in gradient position (0..1).")]
        [SerializeField] private UIOscillation pulse;

        public Gradient ColorGradient => colorGradient;
        public bool UseOriginalColorAsStart => useOriginalColorAsStart;
        public UIOscillation Pulse => pulse;
    }
}
