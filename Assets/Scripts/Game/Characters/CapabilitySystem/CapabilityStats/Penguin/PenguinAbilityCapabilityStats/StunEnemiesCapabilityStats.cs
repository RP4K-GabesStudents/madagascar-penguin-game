using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.Penguin.PenguinAbilityCapabilityStats
{
    [CreateAssetMenu(fileName =  "StunEnemiesCapabilityStats", menuName = "Characters/CapabilityStats/StunEnemiesCapabilityStats")]
    public class StunEnemiesCapabilityStats : Characters.CapabilityStats
    {
        [SerializeField] private float stunDuration;
        [SerializeField] private float stunRadius;
        [SerializeField] private float cooldownTimer;
        
        public float StunDuration => stunDuration;
        public float StunRadius => stunRadius;
        public float CooldownTimer => cooldownTimer;
    }
}
