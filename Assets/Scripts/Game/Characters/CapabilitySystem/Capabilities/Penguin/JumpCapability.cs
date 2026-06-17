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

        private InventoryCapability _inventory; // for equipped-weapon modifiers

        protected override void OnBound()
        {
            base.OnBound();

            _stats = genericStats as JumpCapabilityStats;
            if (_stats == null) { Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(JumpCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject); }

            _owner.TryAddDataKey(CapabilityKeys.IsCrouching, 1); //force always grounded

            _rigidbody = _owner.rigidbody;
            _animator = _owner.GetComponent<Animator>();
            _inventory = GetComponent<InventoryCapability>();
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
            return !_jumpOnCooldown &&
                   (_owner.GetDataDictionaryValue(CapabilityKeys.IsGrounded).IntAsBool() || _jumpCount <= MaxJumps);
        }

        protected override void Execute()
        {
            if (_owner.GetDataDictionaryValue(CapabilityKeys.IsGrounded).IntAsBool()) _jumpCount = 1;
            else _jumpCount += 1;

            _rigidbody.AddForce(transform.up * JumpPower, ForceMode.Impulse);
            _animator.SetTrigger(StaticUtilities.JumpingAnimID);
            StartCoroutine(HandleCooldown());
        }

        // ---- Equipped-weapon modifiers ----

        private float JumpPower
        {
            get
            {
                var weapon = _inventory ? _inventory.EquippedWeapon : null;
                float mul = weapon && weapon.Stats ? weapon.Stats.JumpPowerMultiplier : 1f;
                return _stats.JumpPower * mul;
            }
        }

        private int MaxJumps
        {
            get
            {
                var weapon = _inventory ? _inventory.EquippedWeapon : null;
                int extra = weapon && weapon.Stats ? weapon.Stats.AdditionalAirJumps : 0;
                return _stats.MaxJumps + extra;
            }
        }
    }
}