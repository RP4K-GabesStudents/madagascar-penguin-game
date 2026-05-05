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

        [SerializeField] private float radius;
        private readonly Collider[] hitColliders = new Collider[10];
        [SerializeField] private LayerMask hitedLayerMask;
        [SerializeField, Range(0, 1)] private float repulsionPercent = 0.5f;

        public void StartMoving()
        {
            enabled = true;
        }

        public void StopMoving()
        {
            enabled = false;
        }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Vector3 forward = TargetPosition - gameObject.transform.position;
            float dist = forward.magnitude;
            int hits = Physics.OverlapSphereNonAlloc(gameObject.transform.position, radius, hitColliders, hitedLayerMask);
            Vector3 repulsion = Vector3.zero;
            for (int i = 0; i < hits; i++)
            {
                Vector3 hitedDist = hitColliders[i].ClosestPoint(gameObject.transform.position);
                Vector3 line = gameObject.transform.position - hitedDist;
                repulsion += line;
            }

            if (hits > 0)
                repulsion = repulsion.normalized * (repulsionPercent * GetAIMovementStats.Speed);

            Vector3 direction = forward / dist;

            Rigidbody.AddForce(repulsion, ForceMode.Impulse);
            Rigidbody.AddForce(direction * GetAIMovementStats.Speed, ForceMode.Impulse);

            Rigidbody.linearVelocity = GetAIMovementStats.GetAdjustedSpeed(Rigidbody.linearVelocity, out _);
        }

        private void OnDrawGizmos()
        {
            Vector3 position = transform.position;

            // 1. Avoidance detection sphere
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
            Gizmos.DrawSphere(position, radius);
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(position, radius);

            // 2. Direction to target and distance
            if (Application.isPlaying)
            {
                Vector3 forward = TargetPosition - position;
                float dist = forward.magnitude;

                // Line to target
                Gizmos.color = Color.green;
                Gizmos.DrawLine(position, TargetPosition);

                // Target cross marker
                float markerSize = 0.3f;
                Gizmos.DrawLine(TargetPosition - Vector3.right * markerSize, TargetPosition + Vector3.right * markerSize);
                Gizmos.DrawLine(TargetPosition - Vector3.up * markerSize, TargetPosition + Vector3.up * markerSize);
                Gizmos.DrawLine(TargetPosition - Vector3.forward * markerSize, TargetPosition + Vector3.forward * markerSize);
                // Distance label in Scene view
                UnityEditor.Handles.Label(Vector3.Lerp(position, TargetPosition, 0.5f), $"dist: {dist:F1}m");

                // 3. Repulsion vectors per hit collider
                int hits = Physics.OverlapSphereNonAlloc(position, radius, hitColliders, hitedLayerMask);
                for (int i = 0; i < hits; i++)
                {
                    Vector3 closestPoint = hitColliders[i].ClosestPoint(position);
                    Vector3 repulsionLine = position - closestPoint;

                    // Closest point dot
                    Gizmos.color = Color.gray;
                    Gizmos.DrawSphere(closestPoint, 0.08f);

                    // Repulsion vector
                    Gizmos.color = new Color(1f, 0.65f, 0f); // orange
                    Gizmos.DrawLine(position, position + repulsionLine.normalized * radius);
                }

                // 4. Current velocity (clamped result)
                if (Rigidbody != null)
                {
                    Gizmos.color = new Color(0.4f, 0.2f, 0.9f); // purple
                    Gizmos.DrawLine(position, position + Rigidbody.linearVelocity);
                }
            }
            else
            {
                // Edit mode: just show the sphere and a label
                UnityEditor.Handles.Label(position + Vector3.up * (radius + 0.2f),
                    $"radius: {radius:F1}");
            }
        }
    }
}