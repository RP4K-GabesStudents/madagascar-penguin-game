using UnityEngine;

namespace Detection.Vision
{
    [CreateAssetMenu(fileName = "VisualDetectionStats", menuName = "Detection/Visual Detection Stats", order = 100)]
    public class VisualDetectionStats : ScriptableObject
    {
        
        [SerializeField] private int trackedTargetLimit = 5;
        [SerializeField, Min(0)] private float detectRange = 10;
        [SerializeField, Range(-1, 1)] private float detectAngle = 0.8f;
        [SerializeField] private LayerMask detectLayerMask;
        [SerializeField] private LayerMask blockingLayerMask;

        public int  TrackedTargetLimit => trackedTargetLimit;
        public float DetectRange => detectRange;
        public float DetectAngle => detectAngle;
        public LayerMask DetectLayerMask => detectLayerMask;
        public LayerMask BlockingLayerMask => blockingLayerMask;
        
    }
}
