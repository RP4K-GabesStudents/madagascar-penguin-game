using UnityEngine;

namespace Game.Characters.Capabilities
{
    public class MovementCapabilityStats : CapabilityStats
    {
       // public override Type GetCapabilityType() => typeof(GroundMovementCapabilityStats);
       
       [Header("Movement")]
       [SerializeField] private float speed = 0;
       [SerializeField] private float maxSpeed = 0;
       
       public float Speed => speed;
       public float MaxSpeed => maxSpeed;
    }
}