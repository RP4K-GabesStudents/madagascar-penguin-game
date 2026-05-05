using System;
using AI.Movement.Core;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Movement.MovementTypes
{
    public class GroundMovementType : MonoBehaviour, IMovementModule
    {
        private NavMeshAgent agent;
        [field: SerializeField] public AIMovementStats GetAIMovementStats { get; private set; }
        public Rigidbody Rigidbody { get; set; }
        public Vector3 TargetPosition { get; set; }
        public Quaternion TargetRotation { get; set; }
        public event Action DestinationReached;
        public void StartMoving()
        {
            enabled = true;
        }

        public void StopMoving()
        {
            enabled = false;
        }
        
        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
        }
        
        private void FixedUpdate()
        {
            if (!agent.enabled || !Rigidbody.isKinematic) return;
            agent.destination = TargetPosition;
            if (agent.isStopped)
            {
                StopMoving();
                DestinationReached?.Invoke();
                Debug.Log("agent is stopped");
            }
        }
    }
}
