using System;
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
            if(IsServer) OnCollision_ServerRpc();
            else
            {
                _destroyLaser ??= StartCoroutine(DestroyLaser());
                SpawnHit(transform.position, transform.forward);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Server)]
        private void OnCollision_ServerRpc()
        {
            PlayPartice_ClientRpc(transform.position, transform.forward);
            NetworkObject.Despawn(false);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void PlayPartice_ClientRpc(Vector3 position, Vector3 forward)
        {
            _destroyLaser ??= StartCoroutine(DestroyLaser());
            
            if (!IsClient) return; //<< Server doesn't need to do this.
            SpawnHit(position, forward);
        }

        private void SpawnHit(Vector3 position, Vector3 forward)
        {
            bool hit = Physics.Raycast(position + forward * -0.25f, forward, out RaycastHit hitInfo, 0.5f);
            if (hit)
            {
                var te = PoolingManager.SpawnObject(laserStats.LaserSpark.name); //Allow entirely clientside.
                te.transform.SetPositionAndRotation(hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                te.transform.SetParent(hitInfo.transform, true);
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
            if (NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(false);
            }

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
