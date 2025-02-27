using System;
using Scriptable_Objects;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Objects
{
    public class Laser : MonoBehaviour
    {
        [SerializeField]private ProjectileStats laserStats;
        private Transform _transform;
        private Rigidbody _rigidbody;
        [SerializeField] private float curLifeTime;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            curLifeTime -= Time.deltaTime;
        }

        private void Die()
        {
            if (curLifeTime !<= 0) return;
            Destroy(gameObject);
        }

        private void ForwardProjectile()
        {
            _rigidbody.AddForce(transform.forward * laserStats.Speed, ForceMode.Impulse);
            
            Debug.Log(_rigidbody.linearVelocity.magnitude);
            if (_rigidbody.linearVelocity.magnitude > laserStats.MaxSpeed) _rigidbody.linearVelocity = Vector3.ClampMagnitude(_rigidbody.linearVelocity, laserStats.MaxSpeed);
        }
    }
}
