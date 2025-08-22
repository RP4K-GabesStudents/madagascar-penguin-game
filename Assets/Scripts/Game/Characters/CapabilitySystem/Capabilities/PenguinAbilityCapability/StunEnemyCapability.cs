using System.Collections;
using System.Collections.Generic;
using Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats;
using Horse_Foler;
using Managers;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.PenguinAbilityCapability
{
    public class StunEnemyCapability : BaseCapability, IInputSubscriber
    {
        [SerializeField] private Transform castOrigin;
        private List<Horse> _horses;
        private StunEnemiesCapabilityStats _stats;
        private bool _isOnCooldown;
        protected override void OnBound()
        {
            base.OnBound();
            _stats = genericStats as StunEnemiesCapabilityStats;
            if (_stats == null) { Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(SpewCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject); }
        }

        public override bool CanExecute()
        {
            return !_isOnCooldown;
        }

        protected override void Execute()
        {
            StartCoroutine(Stun());
        }

        private IEnumerator Stun()
        {
            var hits = Physics.SphereCast(castOrigin.position, _stats.StunRadius, castOrigin.forward, out RaycastHit hit, Mathf.Infinity, StaticUtilities.EnemyLayer);
            if (hits)
            {
                _horses.Add(hit.collider.gameObject.GetComponent<Horse>());
            }
            foreach (Horse horse in _horses)
            {
                Vector3 stunPoint = horse.transform.position;
                //figure out how to stun the horses
            }
            yield return new WaitForSeconds(_stats.StunDuration);
            
            //figure out how to unstun horse
            
            _horses.RemoveAll(horse => horse);
        }

        public void BindControls(GameControls controls)
        {
            controls.Player.Ability.performed += ctx => TryExecute();
        }
    }
}
