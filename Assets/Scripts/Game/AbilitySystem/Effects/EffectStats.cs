using UnityEngine;
using UnityEngine.Serialization;

namespace Game.AbilitySystem.Effects
{
    [CreateAssetMenu(fileName = "EffectStats", menuName = "Scriptable Objects/EffectStats")]
    public class EffectStats : ScriptableObject
    {
        [SerializeField] private float duration;
        [SerializeField] private float healAmountPerTick;
        [SerializeField] private float burnDamagePerTick;
        [SerializeField] private float damageBuffAmount;
        [SerializeField] private bool isActive;
        [SerializeField] private ParticleSystem effect;
        
        public float Duration => duration;
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }
        public float HealAmount => healAmountPerTick;
        public float BurnDamage => burnDamagePerTick;
        public float DamageBuff => damageBuffAmount;

        public ParticleSystem Effect => effect;
    }
}
