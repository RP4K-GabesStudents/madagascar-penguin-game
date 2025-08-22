using System.Collections;
using Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats;
using Scriptable_Objects.Penguin_Stats;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.PenguinAbilityCapability
{
    public class InvunerableAbilityCapability : BaseCapability, IInputSubscriber
    {
        private bool _isOnCooldown;
        private InvunerableCapabilityStats _stats;
        [SerializeField] private PenguinStats penguinStats;
        [SerializeField] private Rigidbody rb;
        private float _curAbilityTime;
        protected override void OnBound()
        {
            base.OnBound();
            _stats = genericStats as InvunerableCapabilityStats;
            if (_stats == null) { Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(InvunerableCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject); }

        }

        private IEnumerator CooldownTimer()
        {
            _isOnCooldown = true;
            yield return new WaitForSeconds(_stats.CooldownTime);
            _isOnCooldown = false;
        }

        public override bool CanExecute()
        {
            return !_isOnCooldown;
        }

        protected override void Execute()
        {
            while (_curAbilityTime < _stats.AbilityTime)
            {
                //rb.constraints = RigidbodyConstraints.FreezeAll;
                penguinStats.BaseResistance = 1;
                _curAbilityTime += Time.deltaTime;
            }

            //rb.constraints = RigidbodyConstraints.None;
            penguinStats.BaseResistance = 0.3f;
            StartCoroutine(CooldownTimer());
        }

        public void BindControls(GameControls controls)
        {
            controls.Player.Ability.performed += ctx => TryExecute();
        }
    }
}