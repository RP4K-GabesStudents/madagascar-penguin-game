using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats
{
    [CreateAssetMenu(fileName = "ItemScanCapabilitySTats", menuName = "Scriptable Objects/ItemScanCapabilitySTats")]
    public class ItemScanCapabilitySTats : Characters.CapabilityStats
    {
        [SerializeField] private float detectRange;
        [SerializeField] private float cooldownTimer;
        [SerializeField] private float maxRange;
        
        public float DetectRange => detectRange;
        public float Cooldown => cooldownTimer;
        public float MaxRange => maxRange;
    }
}
