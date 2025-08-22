using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats
{
    [CreateAssetMenu(fileName = "SpewCapabilityStats", menuName = "Scriptable Objects/SpewCapabilityStats")]
    public class SpewCapabilityStats : Characters.CapabilityStats
    {
        [SerializeField] private Rigidbody[] potionPrefabs;
        [SerializeField] private float potionForce;
        [SerializeField] private float torqueForce;
        [SerializeField] private float potionCooldownTimer;
        
        public Rigidbody[] PotionPrefabs => potionPrefabs;
        public float PotionForce => potionForce;
        public float TorqueForce => torqueForce;
        public float PotionCooldownTimer => potionCooldownTimer;
    }
}
