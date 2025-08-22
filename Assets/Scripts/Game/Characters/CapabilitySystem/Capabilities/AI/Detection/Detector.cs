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
            Gizmos.color = new Color(1, 0.7764705882f, 0.7960784314f, 1);

            Gizmos.DrawWireSphere(head.position, detectorStats.DetectRange);
            Gizmos.DrawRay(head.position, head.forward * detectorStats.DetectRange);
            Gizmos.color = Color.green;
            float angleInDegrees = Mathf.Acos(detectorStats.DetectAngle) * Mathf.Rad2Deg;
            // Draw four rays forming a cone around the forward direction
            // First pair: rotated around X axis
            Gizmos.DrawRay(head.position,
                Quaternion.Euler(angleInDegrees, 0, 0) * head.forward * detectorStats.DetectRange);
            Gizmos.DrawRay(head.position,
                Quaternion.Euler(-angleInDegrees, 0, 0) * head.forward * detectorStats.DetectRange);

            // Second pair: rotated around Y axis
            Gizmos.DrawRay(head.position,
                Quaternion.Euler(0, angleInDegrees, 0) * head.forward * detectorStats.DetectRange);
            Gizmos.DrawRay(head.position,
                Quaternion.Euler(0, -angleInDegrees, 0) * head.forward * detectorStats.DetectRange);
        }

        //REMINDER: remove targets
        private void HandleVision()
        {
            //almond
            int hits = Physics.OverlapSphereNonAlloc(head.position, detectorStats.DetectRange, _colliders,
                detectorStats.BlockingLayerMask);
            for (int i = 0; i < hits; i++)
            {
                if (_detectables.ContainsKey(_colliders[i]) || !CanSeeTarget(_colliders[i].transform.position)) continue;
                Rigidbody c = _colliders[i].attachedRigidbody;
                if (!c) continue;
                if (c.TryGetComponent(out IDetectable detectable))
                {
                    _detectables.Add(_colliders[i], new DetectableTarget(detectable));
                }
            }
        }

        private bool CanSeeTarget(Vector3 targetPosition)
        {
            Vector3 forward = (targetPosition - head.position).normalized;
            bool canSee = Physics.Raycast(head.position, forward, out var hit, detectorStats.DetectRange, detectorStats.DetectLayerMask);
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
                DetectableTarget current = detectable.Value;
                if (!CanSeeTarget(detectable.Key.transform.position))
                {
                    current.CurTime -= dt;
                    if (current.CurTime <= 0)
                    {
                        _detectables.Remove(detectable.Key);
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



