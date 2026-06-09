using AI.Movement.Core;
using AI.Movement.MovementTypes;
using UnityEngine;

namespace AI.Characters
{
    public class GroosterAI : GenericEnemy
    {
        [SerializeField] private Transform target;
        private FlyingMovementType flyingMovement;

        private void Awake()
        {
            flyingMovement = GetComponent<FlyingMovementType>();
        }

        private void OnEnable()
        {
            if (flyingMovement != null)
            {
                flyingMovement.DestinationReached += OnDestinationReached;
                flyingMovement.StartMoving();
            }
        }

        private void OnDisable()
        {
            if (flyingMovement != null)
            {
                flyingMovement.DestinationReached -= OnDestinationReached;
                flyingMovement.StopMoving();
            }
        }

        private void FixedUpdate()
        {
            if (flyingMovement != null && target != null)
                flyingMovement.TargetPosition = target.position;
        }

        private void OnDestinationReached()
        {
            Debug.Log($"{name} reached target.");
        }
    }
}