using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats
{
    [CreateAssetMenu(fileName = "InvunerableCapabilityStats", menuName = "Scriptable Objects/InvunerableCapabilityStats")]
    public class InvunerableCapabilityStats : Characters.CapabilityStats
    {
        [SerializeField] private float abilityTime;
        [SerializeField] private float cooldownTime;
        
        public float AbilityTime => abilityTime;
        public float CooldownTime => cooldownTime;
    }
}
