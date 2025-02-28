using System;
using Interfaces;
using Scriptable_Objects;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Objects
{
    public class Laser : MonoBehaviour
    {
        [SerializeField]private ProjectileStats laserStats;
        [SerializeField]private new Rigidbody rigidbody;
        private GameObject _oner;
        public void Init(GameObject oner)
        {
            _oner = oner; 
            Destroy(gameObject, 3);
            rigidbody.AddForce(transform.forward * laserStats.Speed, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody hitInfoRigidbody = other.attachedRigidbody;
            if (hitInfoRigidbody != null)
            {
                Debug.Log("aaaa" + hitInfoRigidbody.gameObject.name + ", " + _oner.name);
                if (hitInfoRigidbody.gameObject == _oner) return;
                
                if (hitInfoRigidbody.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(laserStats.Damage, Vector3.zero);
                }
            }
            Destroy(gameObject);
        }
    }
}
