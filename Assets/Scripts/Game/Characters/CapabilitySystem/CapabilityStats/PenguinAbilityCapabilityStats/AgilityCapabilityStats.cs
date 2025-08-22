using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats
{
    [CreateAssetMenu(fileName = "AgilityCapabilityStats", menuName = "Scriptable Objects/AgilityCapabilityStats")]
    public class AgilityCapabilityStats : ScriptableObject
    {
        [SerializeField] private float speedIncrease;
        [SerializeField] private float jumpHeightIncrease;
        
        public float SpeedIncrease => speedIncrease;
        public float JumpHeightIncrease => jumpHeightIncrease;
    }
}
