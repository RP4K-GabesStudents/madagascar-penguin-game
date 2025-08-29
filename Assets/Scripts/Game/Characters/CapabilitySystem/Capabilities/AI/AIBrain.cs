

using System;
using Managers.Movement;
using Managers.Movement.Stats;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.AI
{
    public class AIBrain : MonoBehaviour
    {
        [SerializeField] private AIBrainStats moveTypeStats;
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
            if (moveTypeStats.IdleState)
            {
                _moveTypes[(int)EMovementState.Idle] = Activator.CreateInstance(moveTypeStats.IdleState.CreateTypeObject()) as IMoveType;
                _moveTypes[(int)EMovementState.Idle].Initialize(this, moveTypeStats.IdleState);
            }

            if (moveTypeStats.PatrollingState)
            {
                _moveTypes[(int)EMovementState.Patrolling] = Activator.CreateInstance(moveTypeStats.PatrollingState.CreateTypeObject()) as IMoveType;
                _moveTypes[(int)EMovementState.Patrolling].Initialize(this, moveTypeStats.PatrollingState);

            }

            if (moveTypeStats.ChasingState)
            {
                _moveTypes[(int)EMovementState.Chasing] = Activator.CreateInstance(moveTypeStats.ChasingState.CreateTypeObject()) as IMoveType;
                _moveTypes[(int)EMovementState.Chasing].Initialize(this, moveTypeStats.ChasingState);
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
