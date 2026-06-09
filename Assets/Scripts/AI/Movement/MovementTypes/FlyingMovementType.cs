using System;
using UnityEngine;
using AI.Movement.Core;

namespace AI.Movement.MovementTypes
{
    public class FlyingMovementType : MonoBehaviour, IMovementModule
    {
        [field: SerializeField] public AIMovementStats GetAIMovementStats { get; private set; }
        public Rigidbody Rigidbody { get; set; }
        public Vector3 TargetPosition { get; set; }
        public Quaternion TargetRotation { get; set; }
        public event Action DestinationReached;
        
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed;

        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Obstacle Avoidance")]
        [SerializeField] private float radius;
        [SerializeField] private LayerMask hitedLayerMask;
        [SerializeField, Range(0, 1)] private float repulsionPercent = 0.5f;
        [SerializeField] private float orbitDistance;
        [SerializeField] private float orbitPush;
        [SerializeField] private float repulsionForce = 5f;

        private readonly Collider[] hitColliders = new Collider[10];

        public void StartMoving() => enabled = true;
        public void StopMoving() => enabled = false;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (target != null)
            {
                TargetPosition = target.position;
                Vector3 toTarget = (TargetPosition - transform.position);
                float distanceToTarget = toTarget.magnitude;
                if (distanceToTarget <= 0.1f)
                {
                    DestinationReached?.Invoke();
                }
                else
                {
                    Rigidbody.AddForce(toTarget.normalized * movementSpeed, ForceMode.Acceleration);
                }
                transform.LookAt(TargetPosition);
            }
            
            int hits = Physics.OverlapSphereNonAlloc(transform.position, radius, hitColliders, hitedLayerMask);
            Vector3 repulsion = Vector3.zero;
            float strongestRepulsionFraction = 0.5f;

            for (int i = 0; i < hits; i++)
            {
                Vector3 closestPoint = hitColliders[i].ClosestPoint(transform.position);
                Vector3 away = transform.position - closestPoint;
                float proximityFraction = 1f - Mathf.Clamp01(away.magnitude / radius);
                repulsion += away.normalized * proximityFraction;
                strongestRepulsionFraction = Mathf.Max(strongestRepulsionFraction, proximityFraction);
                if (hitColliders[i].attachedRigidbody != null)
                    hitColliders[i].attachedRigidbody.AddForce(-away.normalized * repulsionForce * proximityFraction, ForceMode.Impulse);
                
                Debug.DrawLine(transform.position, closestPoint, Color.red);
            }

            if (repulsion != Vector3.zero)
                Rigidbody.AddForce(repulsion.normalized * repulsionForce * strongestRepulsionFraction, ForceMode.Impulse);
        }

        // --- Gizmos ---
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            Vector3 position = transform.position;

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.15f);
            Gizmos.DrawSphere(position, radius);
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(position, radius);

            Gizmos.color = new Color(0f, 0.85f, 1f, 0.08f);
            Gizmos.DrawSphere(TargetPosition, orbitDistance);
            Gizmos.color = new Color(0f, 0.85f, 1f, 0.7f);
            Gizmos.DrawWireSphere(TargetPosition, orbitDistance);
        }
    }
}