using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.AI
{
    [CreateAssetMenu(fileName = "DetectorStats", menuName = "Scriptable Objects/DetectorStats")]
    public class DetectorStats : ScriptableObject
    {
        [SerializeField, Min(0)] private float detectRange = 10;
        [SerializeField, Range(-1, 1)] private float detectAngle = 0.8f;
        [SerializeField] private LayerMask detectLayerMask;
        [SerializeField] private LayerMask blockingLayerMask;
        [SerializeField] private float detectionTime;
        [SerializeField] private float chaseTime;
        
        public float DetectRange => detectRange;
        public float DetectAngle => detectAngle;
        public LayerMask DetectLayerMask => detectLayerMask;
        public float DetectionTime => detectionTime;
        public LayerMask BlockingLayerMask => blockingLayerMask;
        public float ChaseTime => chaseTime;
    }
}
