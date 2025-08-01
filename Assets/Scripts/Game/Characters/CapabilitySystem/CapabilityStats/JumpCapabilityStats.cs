using System;
using Game.Characters.Movement;
using UnityEngine;

namespace Game.Characters.Capabilities
{
    [CreateAssetMenu(fileName = "JumpCapabilityStats", menuName = "Characters/CapabilityStats/JumpCapabilityStats")]
    public class JumpCapabilityStats : CapabilityStats
    {
        [Space]
        [SerializeField] private float jumpPower = 5;
        [SerializeField] private float jumpCooldown = 0.2f;
        [SerializeField,Min(1)] private int maxJumps = 1;

        private WaitForSeconds _jumpCooldownDelay;
        private void OnEnable()
        {
            _jumpCooldownDelay = new WaitForSeconds(jumpCooldown);
        }

        public float JumpPower => jumpPower;
        public float JumpCooldown => jumpCooldown;

        public int MaxJumps => maxJumps;

        public WaitForSeconds JumpCooldownDelay => _jumpCooldownDelay;
        
       // public override Type GetCapabilityType() =>  typeof(JumpCapability);
    }
}