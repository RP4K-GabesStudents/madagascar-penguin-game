

using System;
using Managers.Movement;
using Managers.Movement.Stats;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.AI
{
    public class AIBrain : MonoBehaviour
    {
        [SerializeField] private MoveTypeStats idleState;
        [SerializeField] private MoveTypeStats patrollingState;
        [SerializeField] private MoveTypeStats chasingState;
        private readonly IMoveType[] _moveTypes = new IMoveType[4];
        
        private EMovementState _movementState = EMovementState.None;
        private MoveState _activeMoveState;
        public EMovementState CurState { get => _movementState;
            set
            {
                if(_movementState == value) return;
                _movementState = value;
                _activeMoveState = _moveTypes[(int)value].Update;
                _moveTypes[(int)value].OnBecameActive();
            }
        }

        private void Awake()
        {
            if (idleState)
            {
                _moveTypes[(int)EMovementState.Idle] = Activator.CreateInstance(idleState.CreateTypeObject()) as IMoveType;
                _moveTypes[(int)EMovementState.Idle].Initialize(this, idleState);
            }

            if (patrollingState)
            {
                _moveTypes[(int)EMovementState.Patrolling] = Activator.CreateInstance(patrollingState.CreateTypeObject()) as IMoveType;
                _moveTypes[(int)EMovementState.Patrolling].Initialize(this, patrollingState);

            }

            if (chasingState)
            {
                _moveTypes[(int)EMovementState.Chasing] = Activator.CreateInstance(chasingState.CreateTypeObject()) as IMoveType;
                _moveTypes[(int)EMovementState.Chasing].Initialize(this, chasingState);
            }
        }

        private void Update()
        {
            _activeMoveState?.Invoke();
        }
        public enum EMovementState : byte
        {
            None,
            Idle,
            Patrolling,
            Chasing
        }
        public delegate void MoveState();
    }
}
