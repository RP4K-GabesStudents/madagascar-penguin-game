using UnityEngine;
using UnityEngine.Serialization;

namespace AI.Movement.Core
{
    [CreateAssetMenu(fileName = "AIMovementStats", menuName = "Scriptable Objects/AIMovementStats")]
    public class AIMovementStats : ScriptableObject
    {
        [SerializeField] private float speed; 
        [SerializeField] private float maxSpeed;
        [SerializeField] private bool maxSpeedIncludesY;
        
        public float Speed => speed;
        public float MaxSpeed => maxSpeed;
        public bool MaxSpeedIncludesY => maxSpeedIncludesY;

        public Vector3 GetAdjustedSpeed(Vector3 velocity, out float curSpeed)
        {
            float oldSpeed;
            if (maxSpeedIncludesY)
                oldSpeed = velocity.magnitude;
            else
                oldSpeed = Mathf.Sqrt(Mathf.Pow(velocity.x, 2) + Mathf.Pow(velocity.z, 2));
            
            curSpeed = Mathf.Min(oldSpeed, maxSpeed);

            Vector3 newVelocity;
            if (maxSpeedIncludesY)
                newVelocity = velocity.normalized * curSpeed;
            else
                newVelocity = new Vector3(velocity.x/oldSpeed * curSpeed, velocity.y, velocity.z/oldSpeed * curSpeed);

            return newVelocity;
        }
    }
}
