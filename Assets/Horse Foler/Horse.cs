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
        private Animator _animator;
        [SerializeField] private HorseStats horseStats;
        private NavMeshAgent _navMesh;
        [SerializeField] private Transform attackLocation;
        [SerializeField] private ParticleSystem gabesParticles;
        

        private readonly Collider[] _hits = new Collider[10];

        private void Awake()
        {
            _navMesh = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            _navMesh.SetDestination(target.position);
        }
        private void Attack()
        {
            _animator.SetBool(StaticUtilities.AttackAnimID, true);
            var size = Physics.OverlapSphereNonAlloc(attackLocation.position, horseStats.AttackRange, _hits, StaticUtilities.PlayerLayer);
            for (int i = 0; i < size; i++)
            {
                _hits[i].TryGetComponent(out IDamageable damageable);
                damageable.TakeDamage(horseStats.Damage, horseStats.AttackForce);
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

        private void ExecuteAttack()
        {
            
            var size = Physics.OverlapSphereNonAlloc(attackLocation.position, horseStats.AttackRange, _hits, StaticUtilities.PlayerLayer);
            for (int i = 0; i < size; i++)
            {
                _hits[i].TryGetComponent(out IDamageable damageable);
                Vector3 direction = (_hits[i].transform.position - transform.position);
                Vector3 normal = new Vector3(direction.x, 0, direction.z).normalized;
                damageable.TakeDamage(horseStats.Damage, horseStats.AttackForce.x * normal + Vector3.up * horseStats.AttackForce.y);
                Debug.DrawLine(attackLocation.position, _hits[i].transform.position, Color.red, 3f);
            }
            gabesParticles.Play();
        }

        private void OnDrawGizmosSelected()
        {   
            Gizmos.DrawWireSphere(attackLocation.position, horseStats.AttackRange);
        }


        public float Health { get; set; }
        public float DamageRes { get; }
    }
}