using System;
using System.Collections.Generic;
using UnityEngine;

namespace Detection
{
    public class Detector : MonoBehaviour
    {
        public event Action<IDetectable> OnDetected;
        public IDetectable[] CurTargets { get; private set; }
        [SerializeField] private DetectorStats detectorStats;
        [SerializeField] private Transform head;
        private readonly Collider[] _colliders = new Collider[NumDetector];
        public const int NumDetector = 10;
        Dictionary<Collider, float> _detectables = new();

        private void Update()
        {
            HandleVision();
            UpdateTargets();
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
            int hits = Physics.OverlapSphereNonAlloc(head.position, detectorStats.DetectRange, _colliders, detectorStats.BlockingLayerMask);
            for (int i = 0; i < hits; i++)
            {
                bool canSee = Physics.Raycast(head.position, head.forward, out var hit, detectorStats.DetectRange, detectorStats.DetectLayerMask);
                if (!canSee || Vector3.Dot(head.forward, hit.point - head.position) > detectorStats.DetectAngle || _detectables.ContainsKey(_colliders[i])) continue;
                Rigidbody c = _colliders[i].attachedRigidbody;
                if (!c) continue;
                if (c.TryGetComponent(out IDetectable detectable))
                {
                    _detectables.Add(_colliders[i], 0);
                }
            }
        }

        //
        private void UpdateTargets()
        {
            float dt = Time.deltaTime;
            foreach (var detectable in _detectables)
            {
                //detectable.Value *= dt;
            }  
        }
        
    }
}
