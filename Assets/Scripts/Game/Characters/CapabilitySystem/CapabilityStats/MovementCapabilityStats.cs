using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats
{
    [CreateAssetMenu(fileName = "MovementCapabilityStats", menuName = "Characters/CapabilityStats/MovementCapabilityStats")]
    public class MovementCapabilityStats : Characters.CapabilityStats
    {
       // public override Type GetCapabilityType() => typeof(GroundMovementCapabilityStats);
       
       [Header("Movement")]
       [SerializeField] private float speed = 0;
       [SerializeField] private float maxSpeed = 0;
       
       public float Speed => speed;
       public float MaxSpeed => maxSpeed;
    }
}