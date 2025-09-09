using System.Collections;
using Game.Characters.Capabilities;
using Game.Characters.CapabilitySystem.CapabilityStats;
using Game.Characters.CapabilitySystem.CapabilityStats.Penguin;
using Managers;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    public class JumpCapability : BaseCapability, IInputSubscriber 
    {
        private JumpCapabilityStats _stats;

        private float _currentTime;
        private bool _jumpOnCooldown;
        private int _jumpCount;

        private Rigidbody _rigidbody;
        private Animator _animator;
        
        protected override void OnBound()
        {
            base.OnBound();
            
            _stats = genericStats as JumpCapabilityStats;
            if (_stats == null) { Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(JumpCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject); }

            
            _rigidbody = _owner.rigidbody;
            _animator = _owner.GetComponent<Animator>();
        }


        private IEnumerator HandleCooldown()
        {
            _jumpOnCooldown = true;
            yield return _stats.JumpCooldown;
            _jumpOnCooldown = false;
        }

        public void BindControls(GameControls controls)
        {
            controls.Player.Jump.performed += ctx =>
            {
                if (ctx.ReadValueAsButton()) Execute();
            };
        }

        public override bool CanExecute()
        {
            return _owner.GetDataDictionaryValue(CapabilityKeys.IsGrounded).IntAsBool() && !_jumpOnCooldown && _jumpCount <= _stats.MaxJumps;
        }

        protected override void Execute()
        {
            _rigidbody.AddForce(transform.up * _stats.JumpPower, ForceMode.Impulse);
            _animator.SetTrigger(StaticUtilities.JumpingAnimID);
        }
    }
}