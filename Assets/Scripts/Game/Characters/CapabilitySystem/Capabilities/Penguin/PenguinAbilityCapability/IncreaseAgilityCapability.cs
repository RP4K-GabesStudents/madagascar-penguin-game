using Game.Characters.CapabilitySystem.CapabilityStats;
using Game.Characters.CapabilitySystem.CapabilityStats.Penguin;
using Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.Penguin.PenguinAbilityCapability
{
    public class IncreaseAgilityCapability : MonoBehaviour
    {
        [SerializeField] private MovementCapabilityStats movement;
        [SerializeField] private AgilityCapabilityStats agility;
        [SerializeField] private JumpCapabilityStats jump;

        private void Awake()
        {
            movement.Speed += agility.SpeedIncrease;
            jump.JumpPower += agility.JumpHeightIncrease;
        }
    }
}