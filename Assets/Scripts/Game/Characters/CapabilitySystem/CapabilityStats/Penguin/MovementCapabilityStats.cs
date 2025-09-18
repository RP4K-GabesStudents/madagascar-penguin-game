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
       
        [Header("Ground Detection")]
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float groundCheckRadius = 0.5f;
        [SerializeField] private Vector3 groundCheckOffset = Vector3.zero;
       
        public float Speed
        {
            get => speed;
            set => speed = value;
        }

        public float MaxSpeed => maxSpeed;
        public float GroundCheckDistance => groundCheckDistance;
        public float GroundCheckRadius => groundCheckRadius;
        public Vector3 GroundCheckOffset => groundCheckOffset;
    }
}