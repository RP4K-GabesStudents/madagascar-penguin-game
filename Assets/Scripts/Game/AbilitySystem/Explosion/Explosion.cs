using System;
using Game.Objects;
using Managers;
using Managers.Pooling_System;
using Unity.Netcode;
using UnityEngine;

namespace Game.AbilitySystem.Explosion
{
    public class Explosion : NetworkBehaviour, IPoolable
    {
        [SerializeField] private ExplosionStats stats;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private AudioSource audioSource;
        private readonly Collider[] _collider = new Collider[9];
        [SerializeField] private bool isContactExplosive;
        private NetworkVariable<float> _curTime = new();

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            _curTime.Value = stats.ExplosionTime;
        }

        public void Spawn(ulong spawnID)
        {
            NetworkObject.SpawnWithOwnership(spawnID);
        }

        public void ForceDespawn()
        {
            if (NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(false);
            }
        }

        private void Update()
        {
            _curTime.Value -= Time.deltaTime;
            if (_curTime.Value <= 0)
            {
                Explode_Rpc();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (isContactExplosive)
            {
                Explode_Rpc();
            }
        }

        // Already server-authoritative: the damage loop runs here on the server,
        // so each victim's ApplyNetworkedDamage executes locally on the server
        // (no extra hop, no owner gate needed).
        [Rpc(SendTo.Server)]
        private void Explode_Rpc()
        {
            int numHits = Physics.OverlapSphereNonAlloc(transform.position, stats.ExplosionRadius, _collider, StaticUtilities.AttackableLayers);
            for (int i = 0; i < numHits; i++)
            {
                Collider cur = _collider[i];
                Rigidbody rb = cur.attachedRigidbody;
                if ((rb && rb.TryGetComponent(out IDamageable target)) || cur.TryGetComponent(out target))
                {
                    Vector3 difference = cur.transform.position - transform.position;
                    float distance = difference.magnitude;
                    difference /= distance;

                    float percent = Mathf.Clamp01(distance / stats.ExplosionRadius);

                    difference *= stats.ExplosionForce(percent);
                    target.ApplyNetworkedDamage(stats.ExplosionDamage(percent), difference);
                }
            }
            Explode_VisualRpc();
            ForceDespawn();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Explode_VisualRpc()
        {
            particles.Play();
            audioSource.PlayOneShot(stats.Audio);
        }
    }
}