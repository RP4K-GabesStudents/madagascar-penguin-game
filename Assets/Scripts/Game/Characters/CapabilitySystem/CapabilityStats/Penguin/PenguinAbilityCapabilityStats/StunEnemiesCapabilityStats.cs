using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.Penguin.PenguinAbilityCapabilityStats
{
    public class StunEnemiesCapabilityStats : Characters.CapabilityStats
    {
        [SerializeField] private float stunDuration;
        [SerializeField] private float stunRadius;
        [SerializeField] private float cooldownTimer;
        //waving animation
        [SerializeField] private Animator animator;
        
        public float StunDuration => stunDuration;
        public float StunRadius => stunRadius;
        public float CooldownTimer => cooldownTimer;
        public Animator Animator => animator;
        
    }
}
