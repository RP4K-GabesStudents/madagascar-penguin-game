using System.Collections;
using Game.Characters.CapabilitySystem.CapabilityStats.Penguin;
using Managers;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.Penguin
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

            _owner.TryAddDataKey(CapabilityKeys.IsCrouching, 1); //force always grounded
            
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
                if (ctx.ReadValueAsButton()) TryExecute();
            };
        }

        public override bool CanExecute()
        {
            return !_jumpOnCooldown && (_owner.GetDataDictionaryValue(CapabilityKeys.IsGrounded).IntAsBool() || _jumpCount <= _stats.MaxJumps);
        }

        protected override void Execute()
        {
            if (_owner.GetDataDictionaryValue(CapabilityKeys.IsGrounded).IntAsBool()) _jumpCount = 1;
            else _jumpCount += 1;
            
            _rigidbody.AddForce(transform.up * _stats.JumpPower, ForceMode.Impulse);
            _animator.SetTrigger(StaticUtilities.JumpingAnimID);
            StartCoroutine(HandleCooldown());
        }
    }
}