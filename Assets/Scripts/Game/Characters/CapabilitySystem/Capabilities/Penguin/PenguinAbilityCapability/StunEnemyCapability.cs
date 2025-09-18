using System.Collections;
using Game.Characters.CapabilitySystem.CapabilityStats.Penguin.PenguinAbilityCapabilityStats;
using Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats;
using Horse_Foler;
using Managers;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.Penguin.PenguinAbilityCapability
{
    public class StunEnemyCapability : BaseCapability, IInputSubscriber
    {
        private StunEnemiesCapabilityStats _stats;
        private Collider[] _colliders;
        private bool _isOnCooldown;
        protected override void OnBound()
        {
            base.OnBound();
            _stats = genericStats as StunEnemiesCapabilityStats;
            if (_stats == null) { Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(StunEnemiesCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject); }
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
            var hit = Physics.OverlapSphereNonAlloc(transform.position, _stats.StunRadius, _colliders, StaticUtilities.EnemyLayer);
            for (int i = 0; i < hit; i++)
            {
                Collider cur = _colliders[i];
                Rigidbody rb = cur.attachedRigidbody;
                if (rb && rb.TryGetComponent(out Horse horse) || cur.TryGetComponent(out horse))
                {
                    rb.linearVelocity = Vector3.zero;
                    yield return new WaitForSeconds(_stats.StunDuration);
                }
            }
            yield return null;
        }

        public void BindControls(GameControls controls)
        {
            controls.Player.Ability.performed += ctx => TryExecute();
        }
    }
}
