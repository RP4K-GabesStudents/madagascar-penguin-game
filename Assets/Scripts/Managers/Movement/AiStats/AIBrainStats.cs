using UnityEngine;

namespace Managers.Movement.Stats
{
    [CreateAssetMenu(fileName = "AIBrainStats", menuName = "MoveType/AIBrainStats", order = 1)]
    public class AIBrainStats : ScriptableObject
    {
        [SerializeField] private MoveTypeStats idleState;
        [SerializeField] private MoveTypeStats patrollingState;
        [SerializeField] private MoveTypeStats chasingState;
        
        public MoveTypeStats IdleState => idleState;
        public MoveTypeStats PatrollingState => patrollingState;
        public MoveTypeStats ChasingState => chasingState;
    }
}