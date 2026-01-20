using UnityEngine;

namespace AI.GOAP.Agent
{
    [CreateAssetMenu(fileName = "GoatAgentStats", menuName = "Scriptable Objects/GoatAgentStats")]
    public class GoapAgentStats : ScriptableObject
    {
        //Should goapagent control the health and stuff? remove later if not
        
        [SerializeField] private float maxHealth;
        [SerializeField] private float maxStamina;
        [SerializeField] private float time;
        [SerializeField] private float idleTime;
        [SerializeField] private float wanderRadius;
        [SerializeField] private float tries;
        
        public float MaxHealth => maxHealth;
        public float MaxStamina => maxStamina;
        public float Time => time;
        public float IdleTime => idleTime;
        public float WanderRadius => wanderRadius;
        public float Tries => tries;
        
    }
}
