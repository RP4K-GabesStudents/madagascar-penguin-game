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
        //private Detector _detector;
        private GameObject _nearestTarget;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            Move();
            if (stats.KroogHealth <= 0) Die();
        }

        private void Attack()
        {
            animator.Play("KroogAttack");
            
        }

        private void Move()
        {
            
        }

        public void Die()
        {
            Vector3 randdir = Random.onUnitSphere;
            animator.Play("KroogDeath");
            Destroy(gameObject, 10f);
            for (int i = 0; i < stats.MitosisAmount; i++)
            {
                Instantiate(kroogPrefab);
                _rb.AddForce(randdir * stats.MitosisForce, ForceMode.Impulse);
            }
        }

        public void Die(Vector3 force)
        {
            
        }

        public void OnHurt(float amount, Vector3 force)
        {
            
        }

        public float Health { get; set; }
        public float DamageRes { get; }
    }
}
