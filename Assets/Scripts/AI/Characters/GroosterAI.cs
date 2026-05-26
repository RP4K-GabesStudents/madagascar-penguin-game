using System;
using AI.Movement.Core;
using AI.Movement.MovementTypes;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Characters
{
    public class GroosterAI : GenericEnemy
    {
        [SerializeField] private Transform target;
        private IMovementModule movementModule;

        private void Awake()
        {
            movementModule = GetComponent<IMovementModule>();
        }

        private void FixedUpdate()
        {
             movementModule.MoveTo(target.position);
        }
    }
}
