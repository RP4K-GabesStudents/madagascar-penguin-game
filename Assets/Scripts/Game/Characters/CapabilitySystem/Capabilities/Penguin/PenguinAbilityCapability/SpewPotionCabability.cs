using System.Collections;
using Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.PenguinAbilityCapability
{
    public class SpewPotionCapability : BaseCapability, IInputSubscriber
    {
        
        [SerializeField] private Transform potionSpawnPoint;
        private SpewCapabilityStats _stats;
        
        private bool _isOnCooldown;
        public override bool CanExecute()
        {
            return !_isOnCooldown;
        }
        protected override void OnBound()
        {
            base.OnBound();
           _stats = genericStats as SpewCapabilityStats;
           if (_stats == null) { Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(SpewCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject); }
           
        }
        private IEnumerator CooldownTimer()
        {
            _isOnCooldown = true;
            yield return new WaitForSeconds(_stats.PotionCooldownTimer);
            _isOnCooldown = false;
        }

        protected override void Execute()
        {
            foreach (Rigidbody rb in _stats.PotionPrefabs)
            {
                Instantiate(rb, potionSpawnPoint);
                rb.AddForce(Vector3.up * _stats.PotionForce, ForceMode.Impulse);
                rb.AddTorque(Vector3.forward * _stats.TorqueForce, ForceMode.Impulse);
            }
            StartCoroutine(CooldownTimer());
        }

        public void BindControls(GameControls controls)
        {
            controls.Player.Ability.performed += ctx => TryExecute();
        }
        
    }
}