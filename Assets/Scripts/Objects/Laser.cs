using System;
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
            Destroy(gameObject, 3);
            rigidbody.AddForce(_networkObject.transform.forward * laserStats.Speed, ForceMode.Impulse);
            
        }
        private void OnTriggerEnter(Collider other)
        {
            Rigidbody hitInfoRigidbody = other.attachedRigidbody;
            if (hitInfoRigidbody != null)
            {
                
                if ((1 << hitInfoRigidbody.gameObject.layer & _targetLayers) == 0) return;
                
                if (hitInfoRigidbody.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(laserStats.Damage, Vector3.zero);
                }
            }
            bool hit = Physics.Raycast(_networkObject.transform.position + _networkObject.transform.forward * -0.25f, _networkObject.transform.forward, out RaycastHit hitInfo, 0.5f);
            if (hit)
            {
                Debug.Log("hit");
                GameObject t = Instantiate(laserSpark, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), hitInfo.transform);
                Destroy(t, 5);
            }
            Destroy(gameObject);
        }
    }
}
