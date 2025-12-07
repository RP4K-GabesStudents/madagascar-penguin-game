using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utilities
{
    public class DragUtility : MonoBehaviour
    {
        [Header("Input & Detection")] [SerializeField]
        private InputActionReference dragAction;

        [SerializeField] private LayerMask mask;

        [Header("Physics Settings")] [SerializeField]
        private float forceBoost = 100f;

        [SerializeField] private float maxForce = 1000f;

#if UNITY_EDITOR || DEBUG
        [Header("Debug Settings")] [SerializeField]
        private bool showDebugLogs = true;

        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color raycastColor = Color.yellow;
        [SerializeField] private Color dragPathColor = Color.cyan;
        [SerializeField] private Color velocityColor = Color.red;
        [SerializeField] private float gizmoSphereSize = 0.1f;
#endif

        private Camera _mainCamera;
        private Rigidbody _curTarget;
        private const int NumSamples = 21;
        private readonly Vector2[] _points = new Vector2[NumSamples];
        private float[] _deltaTime = new float[NumSamples];
        private int _curIndex = 0;
        private uint _totalTicks = 0;
        private float _dragPlaneDistance; // Distance from camera to drag plane

#if UNITY_EDITOR || DEBUG
        // Debug info
        private Vector3 _lastRaycastOrigin;
        private Vector3 _lastRaycastDirection;
        private float _lastRaycastDistance = 1000f;
        private bool _lastRaycastHit = false;
        private Vector3 _lastHitPoint;
        private Vector3 _lastAppliedForce;
        private Vector3[] _worldPoints = new Vector3[NumSamples];
#endif

        private void Awake()
        {
            dragAction.action.performed += BeginDrag;
            LogDebug("DragUtility initialized");
        }

        private void OnDestroy()
        {
            dragAction.action.performed -= BeginDrag;
        }

        private void FixedUpdate()
        {
            if (_curTarget == null)
            {
                enabled = false;
                return;
            }

            ProcessMouse();
            MovePhysicsCharacter();

            if (++_curIndex == NumSamples)
            {
                _curIndex = 0;
            }

            _totalTicks++;

            LogDebug($"FixedUpdate - Tick: {_totalTicks}, Index: {_curIndex}, Target: {_curTarget.name}");
        }

        private void ProcessMouse()
        {
            Vector2 pointer = Pointer.current.position.ReadValue();
            _points[_curIndex] = pointer;
            _deltaTime[_curIndex] = Time.deltaTime;

#if UNITY_EDITOR || DEBUG
            // Convert screen point to world point for gizmo visualization
            if (_mainCamera != null && _curTarget != null)
            {
                Ray ray = _mainCamera.ScreenPointToRay(pointer);
                _worldPoints[_curIndex] = ray.GetPoint(_dragPlaneDistance);
            }
#endif

            LogDebug($"ProcessMouse - Screen: {pointer}, DeltaTime: {Time.deltaTime:F4}");
        }

        private void MovePhysicsCharacter()
        {
            // Convert screen position to world position, maintaining the original depth
            Vector3 screenPoint = new Vector3(_points[_curIndex].x, _points[_curIndex].y, _dragPlaneDistance);
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPoint);

            LogDebug(
                $"MovePhysicsCharacter - Current Pos: {_curTarget.position}, Target: {worldPos}, PlaneDistance: {_dragPlaneDistance}");

            _curTarget.MovePosition(worldPos);
        }

        private void BeginMomentum()
        {
            int min = (int)Mathf.Min(NumSamples, _totalTicks);
            Vector2 dirSum = Vector2.zero;
            Vector2 prev = _points[_curIndex];

            LogDebug($"BeginMomentum - Samples to analyze: {min}");

            for (int i = 1; i < min; i++)
            {
                int index = (_curIndex - i + NumSamples) % NumSamples;
                Vector2 delta = (_points[index] - prev) / _deltaTime[index];
                dirSum += delta;
                prev = _points[index];

                LogDebug($"  Sample {i}: Index={index}, Delta={delta}, DeltaTime={_deltaTime[index]:F4}");
            }

            dirSum /= (min - 1); // Average the velocity
            dirSum *= forceBoost;

            Vector2 clampedForce = Vector2.ClampMagnitude(dirSum, maxForce);
            LogDebug($"Calculated Force - Raw: {dirSum}, Clamped: {clampedForce}");

            // Convert 2D screen velocity to 3D world force
            Vector3 force3D = new Vector3(clampedForce.x, clampedForce.y, 0);
            Vector3 finalForce = Quaternion.LookRotation(transform.right) * force3D;

#if UNITY_EDITOR || DEBUG
            _lastAppliedForce = finalForce;
#endif

            _curTarget.useGravity = true;
            _curTarget.AddForce(finalForce, ForceMode.Impulse);
            LogDebug($"Applied Force: {finalForce} to {_curTarget.name}");
            _curTarget = null;
        }

        private void BeginDrag(InputAction.CallbackContext obj)
        {
            bool isPressed = obj.ReadValueAsButton();
            LogDebug($"BeginDrag called - Button pressed: {isPressed}");

            if (isPressed)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    Debug.LogError("Main camera not found!");
                    return;
                }

                _totalTicks = 0;
                _curTarget = null;
                enabled = false;

                Vector2 screenPosition = Pointer.current.position.ReadValue();
                Ray ray = _mainCamera.ScreenPointToRay(screenPosition);

#if UNITY_EDITOR || DEBUG
                _lastRaycastOrigin = ray.origin;
                _lastRaycastDirection = ray.direction;
                _lastRaycastDistance = 1000f;
#endif

                LogDebug($"Raycasting from screen pos: {screenPosition}");

                if (Physics.Raycast(ray, out RaycastHit hit, 1000, mask))
                {
#if UNITY_EDITOR || DEBUG
                    _lastRaycastHit = true;
                    _lastHitPoint = hit.point;
#endif

                    LogDebug(
                        $"Raycast HIT - Object: {hit.collider.name}, Point: {hit.point}, Distance: {hit.distance}");

                    if (hit.rigidbody)
                    {
                        _curTarget = hit.rigidbody;
                        _curTarget.useGravity = false;
                        // Calculate the distance from camera to the hit point
                        _dragPlaneDistance = Vector3.Distance(_mainCamera.transform.position, hit.point);
                        enabled = true;
                        LogDebug(
                            $"Target acquired via hit.rigidbody: {_curTarget.name}, Plane distance: {_dragPlaneDistance}");
                    }
                    else if (hit.collider.TryGetComponent(out Rigidbody rb))
                    {
                        _curTarget = rb;
                        _curTarget.useGravity = false;
                        // Calculate the distance from camera to the hit point
                        _dragPlaneDistance = Vector3.Distance(_mainCamera.transform.position, hit.point);
                        enabled = true;
                        LogDebug(
                            $"Target acquired via GetComponent: {_curTarget.name}, Plane distance: {_dragPlaneDistance}");
                    }
                    else
                    {
                        LogDebug("Hit object has no Rigidbody!");
                    }
                }
                else
                {
#if UNITY_EDITOR || DEBUG
                    _lastRaycastHit = false;
#endif
                    LogDebug($"Raycast MISS - LayerMask: {mask.value}");
                }
            }
            else if (_curTarget != null)
            {
                LogDebug("Drag released - calculating momentum");
                BeginMomentum();
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEBUG")]
        private void LogDebug(string message)
        {
#if UNITY_EDITOR || DEBUG
            if (showDebugLogs)
            {
                Debug.Log($"[DragUtility] {message}");
            }
#endif
        }

#if UNITY_EDITOR || DEBUG
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // Draw raycast visualization
            if (_lastRaycastOrigin != Vector3.zero)
            {
                Gizmos.color = raycastColor;
                Vector3 endPoint = _lastRaycastHit
                    ? _lastHitPoint
                    : _lastRaycastOrigin + _lastRaycastDirection * _lastRaycastDistance;
                Gizmos.DrawLine(_lastRaycastOrigin, endPoint);

                if (_lastRaycastHit)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(_lastHitPoint, gizmoSphereSize * 2);
                }
            }

            // Draw drag path (screen space projected to world)
            if (_curTarget != null && _mainCamera != null)
            {
                Gizmos.color = dragPathColor;
                for (int i = 0; i < NumSamples - 1; i++)
                {
                    if (_worldPoints[i] != Vector3.zero && _worldPoints[i + 1] != Vector3.zero)
                    {
                        Gizmos.DrawLine(_worldPoints[i], _worldPoints[i + 1]);
                    }
                }

                // Draw current position
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(_curTarget.position, gizmoSphereSize * 3);
            }

            // Draw last applied force vector
            if (_lastAppliedForce != Vector3.zero && _curTarget != null)
            {
                Gizmos.color = velocityColor;
                Vector3 forceStart = _curTarget.position;
                Vector3 forceEnd = forceStart + _lastAppliedForce.normalized * 2f;
                Gizmos.DrawLine(forceStart, forceEnd);
                Gizmos.DrawWireSphere(forceEnd, gizmoSphereSize);
            }
        }
#endif
    }
}