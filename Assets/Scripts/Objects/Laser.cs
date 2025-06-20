using System;
using System.Collections;
using Interfaces;
using Scriptable_Objects;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Objects
{
    public class Laser : NetworkBehaviour
    {
        [SerializeField]private ProjectileStats laserStats;
        [SerializeField]private new Rigidbody rigidbody;
        private GameObject _oner;
        [SerializeField] private GameObject laserSpark;
        private NetworkObject _networkObject;
        private int _targetLayers;

        private void Awake()
        {
            _networkObject = GetComponent<NetworkObject>();
        }

        public void Init(int targetLayers, ulong onerId)
        {
            _targetLayers = targetLayers;
            _networkObject.SpawnWithOwnership(onerId);
            StartCoroutine(DestroyLaser());
            rigidbody.AddForce(_networkObject.transform.forward * laserStats.Speed, ForceMode.Impulse);
            
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
            _networkObject.Despawn();
        }

        [ClientRpc]
        private void PlayPartice_ClientRpc(Vector3 position, Vector3 forward)
        {
            bool hit = Physics.Raycast(position + forward * -0.25f, forward, out RaycastHit hitInfo, 0.5f);
            if (hit)
            { 
                GameObject t = Instantiate(laserSpark, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), hitInfo.transform);
                Destroy(t, 5);
            }
            Debug.Log("i eat oranges for bnreakfast");
        }

        private static readonly WaitForSeconds Wait = new WaitForSeconds(3);
        private IEnumerator DestroyLaser()
        {
            yield return Wait;
            _networkObject.Despawn();
        }
    }
}
