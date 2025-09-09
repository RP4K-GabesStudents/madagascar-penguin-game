using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.Penguin
{
    [CreateAssetMenu(fileName = "MovementCapabilityStats", menuName = "Characters/CapabilityStats/MovementCapabilityStats")]
    public class MovementCapabilityStats : Characters.CapabilityStats
    {
       // public override Type GetCapabilityType() => typeof(GroundMovementCapabilityStats);
       
       [Header("Movement")]
       [SerializeField] private float speed = 0;
       [SerializeField] private float maxSpeed = 0;
       
       public float Speed
       {
           get => speed;
           set => speed = value;
       }

       public float MaxSpeed => maxSpeed;
    }
}