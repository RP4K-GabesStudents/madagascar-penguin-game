using UnityEngine;

namespace AI.GOAP.Sensor
{
    [CreateAssetMenu(fileName = "SensorStats", menuName = "Scriptable Objects/SensorStats")]
    public class SensorStats : ScriptableObject
    {
        [SerializeField] private float detectionRadius;
        [SerializeField] private float timerInterval;
        
        public float DetectionRadius => detectionRadius;
        public float TimerInterval => timerInterval;
    }
}
