using System;
using UnityEngine;

namespace AI.Movement.Core
{
    public interface IMovementModule
    {
        public AIMovementStats GetAIMovementStats { get; }
        public Rigidbody Rigidbody { get; set; }
        public Vector3 TargetPosition { get; set; }
        public Quaternion TargetRotation { get; set; }
        public event Action DestinationReached;


        public void MoveTo(Vector3 targetPosition, Quaternion rotation)
        {
            TargetPosition = targetPosition;
            TargetRotation = rotation;
            StartMoving();
        }

        public void MoveTo(Vector3 targetPosition)
        {
            MoveTo(targetPosition, Rigidbody.rotation);
        }
        
        public void StartMoving();
        public void StopMoving();
    }
}
