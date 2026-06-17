using UnityEngine;

namespace UI.Effects.ScriptableObjects
{
    [CreateAssetMenu(fileName = "RotateAnimationData", menuName = "GabesCommonUtility/UI/Rotate Animation Data")]
    public class RotateAnimationDataSo : AnimationDataSoBase
    {
        [Tooltip("Z rotation in degrees applied while active, relative to the resting rotation.")]
        [SerializeField] private float rotationZ = 0f;
        [Tooltip("If true, rotationZ is an absolute local Z angle instead of a delta.")]
        [SerializeField] private bool useAbsoluteRotation = false;
        [Tooltip("Optional continuous swing layered on top of the held tilt, in degrees around the effect's axis.")]
        [SerializeField] private UIOscillation swing;

        public float RotationZ => rotationZ;
        public bool UseAbsoluteRotation => useAbsoluteRotation;
        public UIOscillation Swing => swing;

        // Linear by default reads better for rotation than the inherited ease.
        private void Reset() => transitionCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }
}
