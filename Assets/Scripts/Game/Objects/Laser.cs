using System.Collections;
using Interfaces;
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
        

        private GameObject _oner;
        private int _targetLayers;
        public void Init(int targetLayers)
        {
            _targetLayers = targetLayers;
            StartCoroutine(DestroyLaser());
            rigidbody.linearVelocity = transform.forward * laserStats.Speed;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            Rigidbody hitInfoRigidbody = other.attachedRigidbody;
            if (hitInfoRigidbody != null)
            {
                
                if ((1 << hitInfoRigidbody.gameObject.layer & _targetLayers) == 0) return;
                
                if (hitInfoRigidbody.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(laserStats.Damage, Vector3.zero);
                }
            }
            PlayPartice_ClientRpc(transform.position, transform.forward);
            if (NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(false);
            }
        }

        [ClientRpc]
        private void PlayPartice_ClientRpc(Vector3 position, Vector3 forward)
        {
            bool hit = Physics.Raycast(position + forward * -0.25f, forward, out RaycastHit hitInfo, 0.5f);
            if (hit)
            {
                var te = PoolingManager.SpawnObject(laserStats.LaserSpark.name);
                te.transform.SetPositionAndRotation(hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                te.transform.SetParent(hitInfo.transform, true);
            }
            Debug.Log("i eat oranges for bnreakfast");
        }

        private static readonly WaitForSeconds Wait = new WaitForSeconds(3);
        private IEnumerator DestroyLaser()
        {
            yield return Wait;
            if (NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(false);
            }
        }

        public void Spawn(ulong spawnID)
        {
            StopAllCoroutines();
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
