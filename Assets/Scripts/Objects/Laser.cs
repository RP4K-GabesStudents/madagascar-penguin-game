using System;
using Interfaces;
using Scriptable_Objects;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Objects
{
    public class Laser : MonoBehaviour
    {
        [SerializeField]private ProjectileStats laserStats;
        [SerializeField]private new Rigidbody rigidbody;
        private GameObject _oner;
        [SerializeField] private GameObject laserSpark;
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
            bool hit = Physics.Raycast(transform.position + transform.forward * -0.25f, transform.forward, out RaycastHit hitInfo, 0.5f);
            GameObject t = Instantiate(laserSpark, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), other.transform);
            Destroy(t, 5);
            Destroy(gameObject);
        }
    }
}
