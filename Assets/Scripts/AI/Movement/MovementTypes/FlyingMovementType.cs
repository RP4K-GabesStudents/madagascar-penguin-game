using System;
using UnityEngine;
using AI.Movement.Core;
using UnityEngine.Serialization;

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

        [SerializeField] private float orbitDistance;
        [SerializeField] private float orbitPush;
        [SerializeField] private bool debugLog;

        [SerializeField] private float probeLength = 5f;
        [SerializeField] private float probeSideAngle = 45f;
        [SerializeField] private float probeVerticalAngle = 35f;
        [SerializeField] private LayerMask wallLayerMask;
        [SerializeField] private float wanderStrength = 1f;
        [SerializeField] private float wanderChangeInterval = 1.2f;

        private static readonly Vector3[] ProbeDirections = new Vector3[]
        {
            Vector3.forward, // straight
            Quaternion.Euler(0, 45, 0) * Vector3.forward, // right
            Quaternion.Euler(0, -45, 0) * Vector3.forward, // left
            Quaternion.Euler(35, 0, 0) * Vector3.forward, // up
            Quaternion.Euler(-35, 0, 0) * Vector3.forward, // down
            Quaternion.Euler(0, 90, 0) * Vector3.forward, // hard right
            Quaternion.Euler(0, -90, 0) * Vector3.forward, // hard left
        };

        private Vector3 _wanderDirection = Vector3.zero;
        private float _wanderTimer = 0f;
        private bool _wasBlockedLastFrame = false;

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
            Vector3 toTarget = TargetPosition - transform.position;
            float dist = toTarget.magnitude;
            Vector3 targetDirection = toTarget / dist;
            int hits = Physics.OverlapSphereNonAlloc(transform.position, radius, hitColliders, hitedLayerMask);
            Vector3 repulsion = Vector3.zero;
            float strongestRepulsionFraction = 0f;

            for (int i = 0; i < hits; i++)
            {
                Vector3 closestPoint = hitColliders[i].ClosestPoint(transform.position);
                Vector3 away = transform.position - closestPoint;
                float proximityFraction = 1f - Mathf.Clamp01(away.magnitude / radius);
                repulsion += away.normalized * proximityFraction;
                strongestRepulsionFraction = Mathf.Max(strongestRepulsionFraction, proximityFraction);
            }

            Vector3 probeDir = GetBestProbeDirection(targetDirection);
            bool allProbesBlocked = probeDir == Vector3.zero;
            _wanderTimer -= Time.fixedDeltaTime;
            if (allProbesBlocked || _wasBlockedLastFrame)
            {
                if (_wanderTimer <= 0f)
                {
                    _wanderDirection = new Vector3(
                        UnityEngine.Random.Range(-1f, 1f),
                        UnityEngine.Random.Range(-0.5f, 0.5f),
                        UnityEngine.Random.Range(-1f, 1f)
                    ).normalized;
                    _wanderTimer = wanderChangeInterval;
                }
            }
            else
            {
                _wanderDirection = Vector3.zero;
                _wanderTimer = 0f;
            }

            _wasBlockedLastFrame = allProbesBlocked;
            Vector3 steerDirection;
            if (allProbesBlocked)
            {
                steerDirection = (_wanderDirection + repulsion.normalized).normalized;
            }
            else
            {
                // We have a clear probe direction. steer toward it
                float repulsionWeight = repulsionPercent * strongestRepulsionFraction;
                Vector3 avoidanceBlend = Vector3.Lerp(probeDir, repulsion.normalized, repulsionWeight);
                steerDirection = Vector3.Lerp(avoidanceBlend, probeDir, 0.7f).normalized;
                if (_wanderDirection != Vector3.zero)
                    steerDirection = Vector3.Lerp(steerDirection, _wanderDirection, 0.25f).normalized;
            }

            if (dist <= orbitDistance) steerDirection = Vector3.Lerp(steerDirection, -targetDirection, 0.6f).normalized;
            Rigidbody.AddForce(steerDirection * GetAIMovementStats.Speed, ForceMode.Impulse);
            Rigidbody.linearVelocity = GetAIMovementStats.GetAdjustedSpeed(Rigidbody.linearVelocity, out _);
            transform.LookAt(TargetPosition);

            if (debugLog)
            {
                string state = allProbesBlocked ? "WANDERING" : dist <= orbitDistance ? "REPELLING" : "APPROACHING";
                Debug.Log(
                    $"[FlyingMovement] {gameObject.name} | state: {state} | " +
                    $"dist: {dist:F2} | steer: {steerDirection:F2} | " +
                    $"probeDir: {probeDir:F2} | wander: {_wanderDirection:F2} | " +
                    $"speed: {Rigidbody.linearVelocity.magnitude:F2} | hits: {hits}",
                    gameObject
                );
            }
        }


        private Vector3 GetBestProbeDirection(Vector3 targetDirection)
        {
            Quaternion towardTarget = Quaternion.LookRotation(targetDirection);
            Vector3 bestDir = Vector3.zero;
            float bestScore = float.MinValue;
            foreach (Vector3 localDir in ProbeDirections)
            {
                Vector3 worldDir = towardTarget * localDir;
                bool hit = Physics.Raycast(transform.position, worldDir, out RaycastHit hitInfo, probeLength,
                    wallLayerMask);
                if (!hit)
                {
                    float score = Vector3.Dot(worldDir, targetDirection);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestDir = worldDir;
                    }
                }
            }

            return bestDir;
        }

        private void OnDrawGizmos()
        {
            Vector3 position = transform.position;

            // 1. Avoidance detection sphere (red)
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.15f);
            Gizmos.DrawSphere(position, radius);
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(position, radius);

            // 2. Orbit boundary around target (cyan)
            //    In edit mode TargetPosition is Vector3.zero, so draw around that
            //    In play mode it reflects the real target
            Gizmos.color = new Color(0f, 0.85f, 1f, 0.08f);
            Gizmos.DrawSphere(TargetPosition, orbitDistance);
            Gizmos.color = new Color(0f, 0.85f, 1f, 0.7f);
            Gizmos.DrawWireSphere(TargetPosition, orbitDistance);

            if (Application.isPlaying)
            {
                Vector3 forward = TargetPosition - position;
                float dist = forward.magnitude;
                Vector3 direction = forward / dist;
                bool insideOrbit = dist <= orbitDistance;

                // 3. Line to target — green outside orbit, red inside (pushing back)
                Gizmos.color = insideOrbit ? Color.red : Color.green;
                Gizmos.DrawLine(position, TargetPosition);

                // Target cross marker
                float markerSize = 0.3f;
                Gizmos.DrawLine(TargetPosition - Vector3.right * markerSize,
                    TargetPosition + Vector3.right * markerSize);
                Gizmos.DrawLine(TargetPosition - Vector3.up * markerSize, TargetPosition + Vector3.up * markerSize);
                Gizmos.DrawLine(TargetPosition - Vector3.forward * markerSize,
                    TargetPosition + Vector3.forward * markerSize);

                // Distance label + orbit state
                string orbitLabel = insideOrbit ? "REPELLING" : "approaching";
                UnityEditor.Handles.Label(
                    Vector3.Lerp(position, TargetPosition, 0.5f),
                    $"dist: {dist:F1}m  [{orbitLabel}]"
                );

                // 4. Active force arrows from agent origin
                //    Forward drive (always present)
                Gizmos.color = Color.green;
                Gizmos.DrawLine(position, position + direction * GetAIMovementStats.Speed);

                //    Orbit pushback (only when inside orbit radius)
                if (insideOrbit)
                {
                    Gizmos.color = new Color(1f, 0.3f, 0.8f); // pink = pushback
                    Gizmos.DrawLine(position, position + (-direction * orbitPush));
                }

                // 5. Repulsion vectors per hit collider (orange)
                int hits = Physics.OverlapSphereNonAlloc(position, radius, hitColliders, hitedLayerMask);
                for (int i = 0; i < hits; i++)
                {
                    Vector3 closestPoint = hitColliders[i].ClosestPoint(position);
                    Vector3 repulsionLine = position - closestPoint;

                    Gizmos.color = Color.gray;
                    Gizmos.DrawSphere(closestPoint, 0.08f);

                    Gizmos.color = new Color(1f, 0.65f, 0f);
                    Gizmos.DrawLine(position, position + repulsionLine.normalized * radius);
                }

                // 6. Clamped velocity (purple)
                if (Rigidbody != null)
                {
                    Gizmos.color = new Color(0.4f, 0.2f, 0.9f);
                    Gizmos.DrawLine(position, position + Rigidbody.linearVelocity);
                }
            }
            else
            {
                // Edit mode labels
                UnityEditor.Handles.Label(
                    position + Vector3.up * (radius + 0.2f),
                    $"avoidance radius: {radius:F1}"
                );
                UnityEditor.Handles.Label(
                    TargetPosition + Vector3.up * (orbitDistance + 0.2f),
                    $"orbit distance: {orbitDistance:F1}"
                );
            }

            // Inside your existing OnDrawGizmos, in the Application.isPlaying block:
            if (Application.isPlaying)
            {
                Vector3 toTarget = TargetPosition - transform.position;
                Quaternion towardTarget = Quaternion.LookRotation(toTarget.normalized);

                foreach (Vector3 localDir in ProbeDirections)
                {
                    Vector3 worldDir = towardTarget * localDir;
                    bool hit = Physics.Raycast(transform.position, worldDir, out RaycastHit hi, probeLength,
                        wallLayerMask);

                    Gizmos.color = hit ? Color.red : Color.cyan;
                    Gizmos.DrawRay(transform.position, worldDir * (hit ? hi.distance : probeLength));
                }
            }
        }
    }
}