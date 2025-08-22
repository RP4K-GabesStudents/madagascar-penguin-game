using System;
using Game.Characters.CapabilitySystem.CapabilityStats;
using Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats;
using Scriptable_Objects.Penguin_Stats;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Characters.CapabilitySystem.Capabilities.PenguinAbilityCapability
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