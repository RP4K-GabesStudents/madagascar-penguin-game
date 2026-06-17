using UnityEngine;

namespace UI.Effects.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MoveAnimationData", menuName = "GabesCommonUtility/UI/Move Animation Data")]
    public class MoveAnimationDataSo : AnimationDataSoBase
    {
        [Tooltip("Anchored-position offset applied while active, relative to the resting position.")]
        [SerializeField] private Vector2 moveOffset = Vector2.zero;
        [Tooltip("If true, moveOffset is treated as an absolute anchored position instead of a delta.")]
        [SerializeField] private bool useAbsolutePosition = false;
        [Tooltip("Optional continuous bob layered on top of the held offset, in pixels along bobAxis.")]
        [SerializeField] private UIOscillation bob;
        [Tooltip("Direction the bob travels in, in anchored space.")]
        [SerializeField] private Vector2 bobAxis = Vector2.up;

        public Vector2 MoveOffset => moveOffset;
        public bool UseAbsolutePosition => useAbsolutePosition;
        public UIOscillation Bob => bob;
        public Vector2 BobAxis => bobAxis;
    }
}
