using System;
using UnityEngine;
using UnityEngine.AI;
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

        [Header("Obstacle Avoidance")]
        [SerializeField] private float radius;
        [SerializeField] private LayerMask hitedLayerMask;
        [SerializeField, Range(0, 1)] private float repulsionPercent = 0.5f;
        [SerializeField] private float orbitDistance;
        [SerializeField] private float orbitPush;

        [Header("Pathfinding")]
        [SerializeField] private float pathRefreshInterval = 0.5f;
        [SerializeField] private float waypointReachedRadius = 1f;
        [SerializeField] private bool debugLog;

        private readonly Collider[] hitColliders = new Collider[10];
        private NavMeshPath _navPath;
        private Vector3[] _waypoints = Array.Empty<Vector3>();
        private int _currentWaypoint;
        private float _pathRefreshTimer;
        private Vector3 _lastTargetPosition;

        public void StartMoving() => enabled = true;
        public void StopMoving() => enabled = false;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            _navPath = new NavMeshPath();

            if (Rigidbody == null)
                Debug.LogError($"{name}: FlyingMovementType requires a Rigidbody on this GameObject.", this);

            if (Rigidbody != null && Rigidbody.isKinematic)
                Debug.LogWarning($"{name}: Rigidbody is kinematic — AddForce will have no effect.", this);
        }

        private void FixedUpdate()
        {
            RefreshPathIfNeeded();

            Vector3 steerTarget = GetCurrentSteerTarget();
            Vector3 toSteerTarget = steerTarget - transform.position;
            float distToSteerTarget = toSteerTarget.magnitude;

            if (distToSteerTarget <= waypointReachedRadius && _currentWaypoint < _waypoints.Length - 1)
            {
                _currentWaypoint++;
                steerTarget = _waypoints[_currentWaypoint];
                toSteerTarget = steerTarget - transform.position;
                distToSteerTarget = toSteerTarget.magnitude;
            }

            Vector3 targetDirection = distToSteerTarget > 0.001f ? toSteerTarget / distToSteerTarget : transform.forward;

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

            Vector3 steerDirection;
            if (hits > 0)
            {
                float repulsionWeight = repulsionPercent * strongestRepulsionFraction;
                steerDirection = Vector3.Lerp(targetDirection, repulsion.normalized, repulsionWeight).normalized;
            }
            else
            {
                steerDirection = targetDirection;
            }

            float distToFinalTarget = (TargetPosition - transform.position).magnitude;
            bool insideOrbit = distToFinalTarget <= orbitDistance;
            if (insideOrbit && orbitDistance > 0f)
            {
                steerDirection = Vector3.Lerp(steerDirection, -targetDirection, 0.6f).normalized;
            }

            Rigidbody.AddForce(steerDirection * GetAIMovementStats.Speed, ForceMode.Impulse);
            Rigidbody.linearVelocity = GetAIMovementStats.GetAdjustedSpeed(Rigidbody.linearVelocity, out _);
            transform.LookAt(steerTarget);

            if (debugLog)
            {
                Debug.Log($"[FlyingMovement] {gameObject.name} | " +
                          $"target: {TargetPosition} | pos: {transform.position} | " +
                          $"waypoint: {_currentWaypoint}/{_waypoints.Length - 1} | " +
                          $"distToTarget: {distToFinalTarget:F2} | orbitDistance: {orbitDistance:F2} | " +
                          $"insideOrbit: {insideOrbit} | steerDir: {steerDirection} | " +
                          $"forceApplied: {steerDirection * GetAIMovementStats.Speed} | " +
                          $"velocityAfterClamp: {Rigidbody.linearVelocity} | hits: {hits}", gameObject);
            }
        }

        private void RefreshPathIfNeeded()
        {
            _pathRefreshTimer -= Time.fixedDeltaTime;
            bool targetMoved = Vector3.Distance(TargetPosition, _lastTargetPosition) > 0.5f;

            if (_pathRefreshTimer > 0f && !targetMoved) return;

            _pathRefreshTimer = pathRefreshInterval;
            _lastTargetPosition = TargetPosition;
            RecalculatePath();
        }

        private void RecalculatePath()
        {
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit startHit, 5f, NavMesh.AllAreas) || !NavMesh.SamplePosition(TargetPosition, out NavMeshHit endHit, 5f, NavMesh.AllAreas))
            {
                if (debugLog) Debug.LogWarning($"[FlyingMovement] {gameObject.name} could not sample NavMesh positions.");
                _waypoints = new[] { TargetPosition };
                _currentWaypoint = 0;
                return;
            }

            NavMesh.CalculatePath(startHit.position, endHit.position, NavMesh.AllAreas, _navPath);

            if (_navPath.status == NavMeshPathStatus.PathComplete || _navPath.status == NavMeshPathStatus.PathPartial)
            {
                _waypoints = _navPath.corners;
                _currentWaypoint = _waypoints.Length > 1 ? 1 : 0;
            }
            else
            {
                if (debugLog) Debug.LogWarning($"[FlyingMovement] {gameObject.name} no NavMesh path found, flying direct.");
                _waypoints = new[] { TargetPosition };
                _currentWaypoint = 0;
            }
        }

        private Vector3 GetCurrentSteerTarget()
        {
            if (_waypoints.Length == 0) return TargetPosition;
            return _waypoints[_currentWaypoint];
        }

        // --- Gizmos ---

        private void OnDrawGizmos()
        {
            Vector3 position = transform.position;

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.15f);
            Gizmos.DrawSphere(position, radius);
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(position, radius);

            Gizmos.color = new Color(0f, 0.85f, 1f, 0.08f);
            Gizmos.DrawSphere(TargetPosition, orbitDistance);
            Gizmos.color = new Color(0f, 0.85f, 1f, 0.7f);
            Gizmos.DrawWireSphere(TargetPosition, orbitDistance);

            if (!Application.isPlaying) return;

            if (_waypoints.Length > 1)
            {
                for (int i = 0; i < _waypoints.Length - 1; i++)
                {
                    Gizmos.color = i < _currentWaypoint ? Color.grey : Color.yellow;
                    Gizmos.DrawLine(_waypoints[i], _waypoints[i + 1]);

                    Gizmos.color = i == _currentWaypoint ? Color.green : Color.yellow;
                    Gizmos.DrawSphere(_waypoints[i + 1], 0.2f);
                }
            }

            if (_waypoints.Length > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(position, GetCurrentSteerTarget());
            }

            if (Rigidbody != null)
            {
                Gizmos.color = new Color(0.4f, 0.2f, 0.9f);
                Gizmos.DrawLine(position, position + Rigidbody.linearVelocity);
            }
        }
    }
}