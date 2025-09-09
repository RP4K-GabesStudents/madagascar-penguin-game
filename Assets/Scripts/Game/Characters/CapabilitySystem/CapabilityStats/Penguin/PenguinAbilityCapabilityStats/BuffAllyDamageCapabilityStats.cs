using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.Penguin.PenguinAbilityCapabilityStats
{
    [CreateAssetMenu(fileName = "BuffAllyDamageCapabilityStats", menuName = "Scriptable Objects/BuffAllyDamageCapabilityStats")]
    public class BuffAllyDamageCapabilityStats : Characters.CapabilityStats
    {
        [SerializeField] private float buffAmount;
        [SerializeField] private float buffRadius;
        
        public float BuffAmount => buffAmount;
        public float BuffRadius => buffRadius;
    }
}
