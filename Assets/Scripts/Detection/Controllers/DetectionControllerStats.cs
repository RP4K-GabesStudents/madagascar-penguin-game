using UnityEngine;

namespace Detection.Controllers
{
    [CreateAssetMenu(fileName = "DetectionControllerStats", menuName = "Detection/Detection Controller Stats", order = 95)]
    public class DetectionControllerStats : ScriptableObject
    {
        [SerializeField] private int maxDetectionsPerDetector = 32;
        [SerializeField] private float detectionTime = 5;
        [SerializeField] private float chaseTime = 5;
        
        public int MaxDetectionsPerDetector => maxDetectionsPerDetector;
        public float DetectionTime => detectionTime;
        public float ChaseTime => chaseTime;
        
    }
}