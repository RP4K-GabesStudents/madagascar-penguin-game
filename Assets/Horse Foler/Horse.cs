using Game.Objects;
using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

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
        private RagdollController _ragdollController;
        
        
        private float _distanceToTarget;
        private float _nearestDistance = float.MaxValue;
        private GameObject _nearestTarget;
        //private Detector _detector;
        
        private readonly Collider[] _hits = new Collider[10];

        private void Awake()
        {
            _ragdollController = GetComponent<RagdollController>();
            _navMesh = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _animator.enabled = true;
        }

        private void Update()
        {
            Move();
            if(horseStats.Health <= 0) Die();
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
            if(_nearestTarget)
              _navMesh.SetDestination(_nearestTarget.transform.position);
        }

        public void Die()
        {
            _ragdollController.SetRagdoll(true);
            Destroy(gameObject, 10f);
        }

        public void Die(Vector3 force)
        {
            _ragdollController.SetRagdoll(true);
            _ragdollController.ApplyForce(force);
            Destroy(gameObject, 10f);
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
            Gizmos.DrawSphere(attackLocation.position, horseStats.DetectionRadius);
        }


        public float Health { get; set; }
        public float DamageRes { get; }
    }
}