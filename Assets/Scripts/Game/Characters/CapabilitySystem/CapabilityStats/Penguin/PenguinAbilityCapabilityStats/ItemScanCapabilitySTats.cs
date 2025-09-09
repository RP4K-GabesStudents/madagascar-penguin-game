using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.Penguin.PenguinAbilityCapabilityStats
{
    [CreateAssetMenu(fileName = "ItemScanCapabilitySTats", menuName = "Scriptable Objects/ItemScanCapabilitySTats")]
    public class ItemScanCapabilitySTats : Characters.CapabilityStats
    {
        [SerializeField] private float detectRange;
        [SerializeField] private float cooldownTimer;
        [SerializeField] private float radius;
        [SerializeField] private float highlightTime;
        
        public float DetectRange => detectRange;
        public float Cooldown => cooldownTimer;
        public float Radius => radius;
        public float HighlightTime => highlightTime;
    }
}
