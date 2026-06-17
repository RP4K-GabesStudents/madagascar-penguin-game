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

        private bool _dead;

        private readonly Collider[] _hits = new Collider[10];

        private void Awake()
        {
            _ragdollController = GetComponent<RagdollController>();
            _navMesh = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _animator.enabled = true;
        }

        public override void OnNetworkSpawn()
        {
            // DECISION: per-instance health seeded from the stats SO. Using
            // horseStats.Health directly would share one pool across all horses
            // (it's a ScriptableObject). Death is now driven by the damage
            // system (Die via the interface), not by polling the SO in Update.
            if (IsServer) Health = horseStats.Health;
        }

        private void Update()
        {
            Move();
        }

        // NOTE: Attack() and ExecuteAttack() are near-duplicates. ExecuteAttack
        // adds directional knockback + particles; Attack is the simpler/older
        // one. Both fixed so they compile, but you likely want to keep only one.
        private void Attack()
        {
            _animator.SetBool(StaticUtilities.AttackAnimID, true);
            var size = Physics.OverlapSphereNonAlloc(attackLocation.position, horseStats.AttackRange, _hits, StaticUtilities.PlayerLayer);
            for (int i = 0; i < size; i++)
            {
                // Guard: TryGetComponent's result was previously ignored, so a
                // non-damageable hit threw a NullReferenceException.
                if (_hits[i].TryGetComponent(out IDamageable damageable))
                    damageable.ApplyNetworkedDamage(horseStats.Damage, horseStats.AttackForce);
            }
        }

        private void Move()
        {
            if (_nearestTarget)
                _navMesh.SetDestination(_nearestTarget.transform.position);
        }

        // Unified death. The interface calls Die(Vector3); the old parameterless
        // Die() (same body minus the force) is removed so there's one path.
        public void Die(Vector3 force)
        {
            if (_dead) return;
            _dead = true;

            Die_Rpc(force);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Die_Rpc(Vector3 force)
        {
            _ragdollController.SetRagdoll(true);
            _ragdollController.ApplyForce(force);
            Destroy(gameObject, 10f);
        }

        public void OnHurt(float amount, Vector3 force)
        {

        }

        // Required per-class IDamageable stub.
        [Rpc(SendTo.Server)]
        public void TakeDamageRpc(float damage, Vector3 force) =>
            ((IDamageable)this).TakeDamageLocal(damage, force);

        private void ExecuteAttack()
        {
            var size = Physics.OverlapSphereNonAlloc(attackLocation.position, horseStats.AttackRange, _hits, StaticUtilities.PlayerLayer);
            for (int i = 0; i < size; i++)
            {
                if (!_hits[i].TryGetComponent(out IDamageable damageable)) continue;

                Vector3 direction = (_hits[i].transform.position - transform.position);
                Vector3 normal = new Vector3(direction.x, 0, direction.z).normalized;
                damageable.ApplyNetworkedDamage(horseStats.Damage, horseStats.AttackForce.x * normal + Vector3.up * horseStats.AttackForce.y);
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
        public float DamageRes => 0;
    }
}