using System;
using Game.Characters.CapabilitySystem.CapabilityStats.Penguin.PenguinAbilityCapabilityStats;
using Managers;
using Scriptable_Objects;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.Penguin.PenguinAbilityCapability
{
    public class BuffAllyDamageCapability : MonoBehaviour
    {
        [SerializeField] private BuffAllyDamageCapabilityStats stats;
        private ProjectileStats _projectileStats;
        private Collider[] _colliders;
        
        private void Update()
        {
            Scan();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, stats.BuffRadius);
        }

        private void Scan()
        {
            var hit =  Physics.OverlapSphereNonAlloc(transform.position, stats.BuffRadius, _colliders, StaticUtilities.PlayerLayer);
            for (int i = 0; i < hit; i++)
            {
                Collider cur = _colliders[i];
                Rigidbody rb = cur.attachedRigidbody;
                if (rb && rb.TryGetComponent(out LaserEyesCapability character) || cur.TryGetComponent(out character))
                {
                    _projectileStats.Damage = stats.BuffAmount;
                }
            }
        }
    }
}
