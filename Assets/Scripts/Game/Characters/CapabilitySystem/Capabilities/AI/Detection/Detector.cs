using System;
using System.Collections.Generic;
using Detection;
using Game.Characters.CapabilitySystem.CapabilityStats.AI;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.AI.Detection
{
    public class Detector : MonoBehaviour
    {
        private static readonly int Fill = Shader.PropertyToID("_Fill");
        public event Action OnDetected;
        public event Action OnLost;
        
        public IDetectable CurTargets { get; private set; }

        [SerializeField] private DetectorStats detectorStats;
        [SerializeField] private Transform head;
        [SerializeField] private MeshRenderer meshRenderer;
        private Material _detectionMaterial;
        private Transform _target;
        private float _curDetectionTime;

        private readonly Collider[] _colliders = new Collider[NumDetector];
        public const int NumDetector = 10;
        Dictionary<Collider, DetectableTarget> _detectables = new();


        private void Awake()
        {
            _detectionMaterial = meshRenderer.material;
        }

        private void Update()
        {
            if (CurTargets != null)
            {
                HandleChaseTimer();
            }
            else
            {
                HandleVision();
                UpdateTargets();
            }
        }

        private void OnDrawGizmos()
        {
            if (head == null) return;
            
            // Draw line to current target if detected and visible
            if (CurTargets != null && _target != null)
            {
                bool canSee = CanSeeTarget(_target.position);
                Gizmos.color = canSee ? new Color(0f, 1f, 0f, 0.8f) : new Color(1f, 0.5f, 0f, 0.6f); // Green if visible, orange if lost
                Gizmos.DrawLine(head.position, _target.position);
                
                // Draw sphere at target
                Gizmos.color = canSee ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0.5f, 0f, 0.3f);
                Gizmos.DrawSphere(_target.position, 0.5f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (head == null || detectorStats == null) return;

            Vector3 origin = head.position;
            float range = detectorStats.DetectRange;
            float angleInDegrees = Mathf.Acos(detectorStats.DetectAngle) * Mathf.Rad2Deg;

            // 1. Draw detection range sphere (outer boundary)
            Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.15f); // Light blue, transparent
            Gizmos.DrawWireSphere(origin, range);

            // 2. Draw forward direction ray
            Gizmos.color = new Color(1f, 1f, 0f, 0.9f); // Bright yellow
            Gizmos.DrawRay(origin, head.forward * range);

            // 3. Draw vision cone edges
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.7f); // Cyan/green
            
            // Vertical cone edges (up/down)
            Vector3 upEdge = Quaternion.Euler(angleInDegrees, 0, 0) * head.forward * range;
            Vector3 downEdge = Quaternion.Euler(-angleInDegrees, 0, 0) * head.forward * range;
            Gizmos.DrawRay(origin, upEdge);
            Gizmos.DrawRay(origin, downEdge);

            // Horizontal cone edges (left/right)
            Vector3 rightEdge = Quaternion.Euler(0, angleInDegrees, 0) * head.forward * range;
            Vector3 leftEdge = Quaternion.Euler(0, -angleInDegrees, 0) * head.forward * range;
            Gizmos.DrawRay(origin, rightEdge);
            Gizmos.DrawRay(origin, leftEdge);

            // 4. Draw diagonal cone edges for better visualization
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f); // Lighter cyan
            Vector3 topRightEdge = Quaternion.Euler(angleInDegrees * 0.7f, angleInDegrees * 0.7f, 0) * head.forward * range;
            Vector3 topLeftEdge = Quaternion.Euler(angleInDegrees * 0.7f, -angleInDegrees * 0.7f, 0) * head.forward * range;
            Vector3 bottomRightEdge = Quaternion.Euler(-angleInDegrees * 0.7f, angleInDegrees * 0.7f, 0) * head.forward * range;
            Vector3 bottomLeftEdge = Quaternion.Euler(-angleInDegrees * 0.7f, -angleInDegrees * 0.7f, 0) * head.forward * range;
            
            Gizmos.DrawRay(origin, topRightEdge);
            Gizmos.DrawRay(origin, topLeftEdge);
            Gizmos.DrawRay(origin, bottomRightEdge);
            Gizmos.DrawRay(origin, bottomLeftEdge);

            // 5. Draw cone arc circles at intervals
            DrawConeArc(origin, head.forward, head.up, angleInDegrees, range * 0.33f, new Color(0f, 1f, 0.5f, 0.3f));
            DrawConeArc(origin, head.forward, head.up, angleInDegrees, range * 0.66f, new Color(0f, 1f, 0.5f, 0.3f));
            DrawConeArc(origin, head.forward, head.up, angleInDegrees, range, new Color(0f, 1f, 0.5f, 0.5f));

            // 6. Show raycast results to potential targets
            if (Application.isPlaying)
            {
                foreach (var detectable in _detectables)
                {
                    Vector3 targetPos = detectable.Key.transform.position;
                    bool canSee = CanSeeTarget(targetPos);
                    float detectionProgress = detectable.Value.CurTime / detectorStats.DetectionTime;
                    
                    // Color based on detection progress: white -> yellow -> red
                    Color lineColor = canSee 
                        ? Color.Lerp(Color.white, Color.red, detectionProgress) 
                        : new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gray if can't see
                    
                    Gizmos.color = lineColor;
                    Gizmos.DrawLine(origin, targetPos);
                    
                    // Draw small sphere at detectable
                    Gizmos.color = new Color(lineColor.r, lineColor.g, lineColor.b, 0.6f);
                    Gizmos.DrawSphere(targetPos, 0.3f);
                }

                // 7. Show current target status
                if (CurTargets != null && _target != null)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.8f); // Bright red
                    Gizmos.DrawLine(origin, _target.position);
                    Gizmos.DrawWireSphere(_target.position, 0.7f);
                    
                    // Show chase timer
                    float chaseProgress = _curDetectionTime / detectorStats.ChaseTime;
                    Gizmos.color = Color.Lerp(Color.red, Color.yellow, chaseProgress);
                    Gizmos.DrawWireSphere(_target.position, 0.5f);
                }
            }

            // 8. Draw distance markers
            Gizmos.color = new Color(1f, 1f, 1f, 0.3f); // White, transparent
            for (int i = 1; i <= 3; i++)
            {
                float distance = range * (i / 3f);
                Gizmos.DrawWireSphere(origin, distance);
            }
        }

        // Helper method to draw arc circles around the cone
        private void DrawConeArc(Vector3 origin, Vector3 forward, Vector3 up, float angleInDegrees, float distance, Color color)
        {
            Gizmos.color = color;
            int segments = 32;
            Vector3 previousPoint = Vector3.zero;
            
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i / (float)segments) * 360f;
                Quaternion rotation = Quaternion.LookRotation(forward, up) * Quaternion.Euler(0, 0, angle);
                Vector3 direction = rotation * Vector3.up * Mathf.Tan(angleInDegrees * Mathf.Deg2Rad) * distance;
                Vector3 point = origin + forward * distance + direction;
                
                if (i > 0)
                {
                    Gizmos.DrawLine(previousPoint, point);
                }
                previousPoint = point;
            }
        }

        //REMINDER: remove targets
        private void HandleVision()
        {
            //almond
            int hits = Physics.OverlapSphereNonAlloc(head.position, detectorStats.DetectRange, _colliders,
                detectorStats.BlockingLayerMask);
            Debug.Log("Targets hit: " + hits);
            for (int i = 0; i < hits; i++)
            {
                if (_detectables.ContainsKey(_colliders[i]) || !CanSeeTarget(_colliders[i].transform.position)) continue;
                Rigidbody c = _colliders[i].attachedRigidbody;
                Debug.Log("Detected target: " + _colliders[i].gameObject.name);
                if (!c) continue;
                if (c.TryGetComponent(out IDetectable detectable))
                {
                    Debug.Log("Detected target in collider: " + _colliders[i].gameObject.name);
                    _detectables.Add(_colliders[i], new DetectableTarget(detectable));
                }
            }
        }

        private bool CanSeeTarget(Vector3 targetPosition)
        {
            Vector3 forward = (targetPosition - head.position).normalized;
            bool canSee = Physics.Raycast(head.position, forward, out var hit, detectorStats.DetectRange, detectorStats.DetectLayerMask);
            Debug.Log("can see target: " + canSee);
            return canSee && Vector3.Dot(head.forward, forward) >= detectorStats.DetectAngle;
        } 

        //
        private void UpdateTargets()
        {
            if (_detectables.Count == 0) return;
            float maxDetection = 0;
            float dt = Time.deltaTime;
            
            foreach (var detectable in _detectables)
            {
                Debug.Log("detectable name: " + detectable.Key.name);
                DetectableTarget current = detectable.Value;
                if (!CanSeeTarget(detectable.Key.transform.position))
                {
                    Debug.Log(CanSeeTarget(detectable.Key.transform.position));
                    current.CurTime -= dt;
                    if (current.CurTime <= 0)
                    {
                        _detectables.Remove(detectable.Key);
                        Debug.Log("detectable: " + detectable.Key.gameObject.name);
                    }
                }
                else
                {
                    if (current.CurTime >= detectorStats.DetectionTime)
                    {
                        if (CurTargets is null)
                        {
                            MarkAsDetected(current.Component);
                            break;
                        }
                        continue;
                    }
                    current.CurTime += dt;
                }
                maxDetection = Mathf.Max(current.CurTime / detectorStats.DetectionTime, maxDetection);
                _detectables[detectable.Key] = current;
            }
            _detectionMaterial.SetFloat(Fill, maxDetection);   
        }

        public void MarkAsDetected(IDetectable detectable)
        {
            _target = ((MonoBehaviour)detectable).transform;
            CurTargets = detectable;
            OnDetected?.Invoke();
            _detectables.Clear();
        }
        
        public void MarkAsLost()
        {
            CurTargets = null;
            OnLost?.Invoke();
        }

        private void HandleChaseTimer()
        {
            if (CanSeeTarget(_target.position))
            {
                _curDetectionTime = detectorStats.ChaseTime;
                return;
            }
            float dt = Time.deltaTime;
            _curDetectionTime -= dt;
            if (_curDetectionTime <= 0)
            {
                MarkAsLost();
            }
        }
        

        private struct DetectableTarget
        {
            public float CurTime;
            public readonly IDetectable Component;

            public DetectableTarget(IDetectable component)
            {
                Component = component;
                CurTime = 0;
            }
        }
    }
}