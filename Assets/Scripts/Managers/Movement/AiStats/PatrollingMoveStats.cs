using System;
using AI.Navigation.Logic;
using Game.Characters.CapabilitySystem.Capabilities.AI;
using Managers.Movement.Stats;
using UnityEngine;
using UnityEngine.AI;

namespace Managers.Movement.AiStats
{
    [CreateAssetMenu(fileName = "PatrollingMoveStats", menuName = "MoveType/Patrolling Move")]
    public class PatrollingMoveStats : MoveTypeStats
    {
        [field: SerializeField]public PatrolPathSoap PathSoap { get;  private set; }

        public override Type CreateTypeObject()
        {
            return typeof(PatrollingMovement);
        }
    }

    public class PatrollingMovement : IMoveType
    {
        private PatrollingMoveStats _moveStats;
        private NavMeshAgent _navMesh;
        private AIBrain _brain;
        private int _curIndex;


        public void Initialize(AIBrain brain, MoveTypeStats stats)
        {
            _brain = brain;
            _navMesh = brain.GetComponent<NavMeshAgent>();
            _moveStats = (PatrollingMoveStats)stats;
        }

        public void Update()
        {
          
        }

        public void OnBecameActive()
        {
            _navMesh.SetDestination(_moveStats.PathSoap.waypoints[_curIndex].position);
            if (_navMesh.isStopped)
            {
                _curIndex = (_curIndex + 1) % _moveStats.PathSoap.waypoints.Length;
                _brain.CurState = AIBrain.EMovementState.Idle;
            }
        }
    }
}
