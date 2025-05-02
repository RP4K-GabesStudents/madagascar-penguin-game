using System;
using Interfaces;
using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Horse_Foler
{
    public class Horse : NetworkBehaviour, IDamageable
    {
        [SerializeField] private Transform target;
        [SerializeField]private Animator animator;
        private HorseStats _horseStats;
        private NavMeshAgent _navMesh;
        [SerializeField] private Transform attackLocation;
        

        private readonly Collider[] _hits = new Collider[10];

        private void Awake()
        {
            _navMesh = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            _navMesh.SetDestination(target.position);
        }
        private void Attack()
        {
            animator.SetBool(StaticUtilities.AttackAnimID, true);
            var size = Physics.OverlapSphereNonAlloc(attackLocation.position, _horseStats.AttackRange, _hits, StaticUtilities.PlayerLayer);
            for (int i = 0; i < size; i++)
            {
                _hits[i].TryGetComponent(out IDamageable damageable);
                damageable.TakeDamage(_horseStats.Damage, _horseStats.AttackForce);
            }
        }
        private void Move()
        {
            
        }

        public void Die()
        {
            
        }

        public void OnHurt(float amount, Vector3 force)
        {
            
        }

        public float Health { get; set; }
        public float DamageRes { get; }
    }
}