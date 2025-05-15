using Interfaces;
using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

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
        
        
        private float _distanceToTarget;
        private float _nearestDistance = float.MaxValue;
        private GameObject _nearestTarget;
        
        private readonly Collider[] _hits = new Collider[10];

        private void Awake()
        {
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
            SetNewTarget();
            _navMesh.SetDestination(_nearestTarget.transform.position);
        }

        private void SetNewTarget()
        {
            bool success = Physics.SphereCast(attackLocation.position, horseStats.DetectionRadius, attackLocation.forward, out RaycastHit hitInfo, StaticUtilities.AttackableLayers);
            if (!success) return;
            foreach (var t in horseStats.Targets)
            {
                _distanceToTarget = Vector3.Distance(transform.position, t.transform.position);
                if (_distanceToTarget  < _nearestDistance)
                {
                    _nearestTarget = t;
                    _nearestDistance = _distanceToTarget;
                }
            }
        }

        public void Die()
        {
            _animator.enabled = false;
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