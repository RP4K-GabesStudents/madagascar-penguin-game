using UnityEngine;

namespace Utilities
{
    public class Tether : MonoBehaviour
    {
        [SerializeField] private float maxDistance = 50f;
    
        private Vector3 _originalPosition;
        private Rigidbody _rb;

        private void Start()
        {
            // Store the object's starting position
            _originalPosition = transform.position;
        
            // Get the Rigidbody component
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            // Calculate distance from original position
            float distance = Vector3.Distance(transform.position, _originalPosition);
        
            // If distance exceeds max, teleport back
            if (distance > maxDistance)
            {
                if (_rb != null)
                {
                    // Reset velocity to prevent continued movement
                    _rb.linearVelocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                
                    // Teleport back using Rigidbody
                    _rb.position = _originalPosition;
                }
                else
                {
                    // Fallback if no Rigidbody
                    transform.position = _originalPosition;
                }
            }
        }
    }
}