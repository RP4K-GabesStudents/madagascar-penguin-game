using System.Collections;
using Managers;
using Managers.Pooling_System;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;

namespace Game.Objects
{
    public class Laser : NetworkBehaviour, IPoolable
    {
        [SerializeField] private ProjectileStats laserStats;
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private TrailRenderer trailRenderer;

        private GameObject _oner;
        private int _targetLayers;
        
        private Coroutine _lifeTime;
        private Coroutine _destroyLaser;
        
        public void Init(int targetLayers)
        {
            _targetLayers = targetLayers;
            rigidbody.isKinematic = false;
            rigidbody.linearVelocity = transform.forward * laserStats.Speed;
            _lifeTime = StartCoroutine(LifeTime());
            trailRenderer.emitting = true;
            trailRenderer.Clear();
        }

        private void OnCollisionEnter(Collision other)
        {
            Rigidbody hitInfoRigidbody = other.rigidbody;
            if (hitInfoRigidbody != null)
            {
                if ((1 << hitInfoRigidbody.gameObject.layer & _targetLayers) == 0) return;

                if (hitInfoRigidbody.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(laserStats.Damage, Vector3.zero);
                }
            }
            _destroyLaser ??= StartCoroutine(DestroyLaser());

            ContactPoint contact = other.GetContact(0);
            if (IsServer) PlayParticle_ClientRpc(contact.point, contact.normal);
            if (IsClient) SpawnHit(contact.point, contact.normal, other.transform);
        }

        [Rpc(SendTo.NotServer, InvokePermission = RpcInvokePermission.Server)]
        private void PlayParticle_ClientRpc(Vector3 point, Vector3 normal)
        {
            // Raycast is only used to recover the local Transform for parenting,
            // not to determine position or orientation
            bool hit = Physics.Raycast(point + normal * 0.5f, -normal, out RaycastHit hitInfo, 1f, StaticUtilities.SurfaceLayers);
            SpawnHit(point, normal, hit ? hitInfo.transform : null);
        }

        private void SpawnHit(Vector3 point, Vector3 normal, Transform hitTransform)
        {
            _destroyLaser ??= StartCoroutine(DestroyLaser());

            var spark = PoolingManager.SpawnObject(laserStats.LaserSpark.name);
            spark.transform.SetPositionAndRotation(
                point,
                Quaternion.LookRotation(Vector3.Reflect(-normal, normal))
            );

            if (hitTransform != null)
            {
                spark.transform.SetParent(hitTransform, true);
            }
        }

        private IEnumerator LifeTime()
        {
            yield return new WaitForSeconds(laserStats.Lifetime);
            _destroyLaser ??= StartCoroutine(DestroyLaser());
            _lifeTime = null;
        }

        private IEnumerator DestroyLaser()
        {
            if(_lifeTime != null) StopCoroutine(_lifeTime);

            trailRenderer.emitting = false;
            rigidbody.isKinematic = true;
            yield return new WaitForSeconds(trailRenderer.time);
            NetworkObject.Despawn(false);
            _destroyLaser = null;
        }

        public void Spawn(ulong spawnID)
        {
            StopAllCoroutines();
            _destroyLaser = null;
            _lifeTime = null;
            
            NetworkObject.SpawnWithOwnership(spawnID);
            Init(StaticUtilities.EnemyAttackLayers);
        }

        public void ForceDespawn()
        {
            if (NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(false);
            }
        }
        public override void OnNetworkDespawn()
        {
            gameObject.SetActive(false);
        }
    }
}
