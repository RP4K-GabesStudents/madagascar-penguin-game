using Game.Entities.EntityStats;
using Game.Objects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Entities
{
    public class Kroog : NetworkBehaviour, IDamageable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private KroogStats stats;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private GameObject kroogPrefab;
        private Rigidbody _rb;
        private NavMeshAgent _navMeshAgent;
        private GameObject _nearestTarget;

        private bool _dead;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public override void OnNetworkSpawn()
        {
            // DECISION 1: live health is per-instance, seeded from the stats SO.
            // Using stats.KroogHealth directly would share one health pool across
            // every Kroog (it's a ScriptableObject). If you actually want the SO
            // value as the source of truth, replace this and the Health property.
            if (IsServer) Health = stats.KroogHealth;
        }

        private void Update()
        {
            Move();
            // Death is now driven by the damage system (Die via the interface),
            // not by polling an SO value here. Left Move() in place.
        }

        private void Attack()
        {
            animator.Play("KroogAttack");
        }

        private void Move()
        {

        }

        // DECISION 2: the interface calls Die(Vector3); route it into the real
        // death logic (was in a separate parameterless Die() that nothing
        // reachable called). Force is currently ignored, mitosis scatters
        // randomly; pass 'force' into the scatter if you want directional split.
        public void Die(Vector3 force)
        {
            if (_dead) return;
            _dead = true;

            animator.Play("KroogDeath");
            Die_Rpc();

            // Mitosis: spawn children as networked objects (was Instantiate with
            // no Spawn, which wouldn't replicate).
            if (IsServer)
            {
                for (int i = 0; i < stats.MitosisAmount; i++)
                {
                    Vector3 dir = Random.onUnitSphere;
                    GameObject child = Instantiate(kroogPrefab, transform.position, Quaternion.identity);
                    if (child.TryGetComponent(out NetworkObject netChild)) netChild.Spawn();
                    if (child.TryGetComponent(out Rigidbody childRb))
                        childRb.AddForce(dir * stats.MitosisForce, ForceMode.Impulse);
                }

                if (IsSpawned) NetworkObject.Despawn();
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Die_Rpc()
        {
            animator.Play("KroogDeath");
            if (particles) particles.Play();
        }

        public void OnHurt(float amount, Vector3 force)
        {

        }

        // Required per-class IDamageable stub.
        [Rpc(SendTo.Server)]
        public void TakeDamageRpc(float damage, Vector3 force) =>
            ((IDamageable)this).TakeDamageLocal(damage, force);

        public float Health { get; set; }
        public float DamageRes => 0;
    }
}