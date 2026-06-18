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

            if (flyingMovement == null)
                Debug.LogError($"{name}: No FlyingMovementType found on this GameObject.", this);
        }

        private void OnEnable()
        {
            if (flyingMovement != null)
                flyingMovement.StartMoving();
        }

        private void OnDisable()
        {
            if (flyingMovement != null)
                flyingMovement.StopMoving();
        }

        private void FixedUpdate()
        {
            if (flyingMovement == null || target == null) return;

            flyingMovement.TargetPosition = target.position;
        }
    }
}