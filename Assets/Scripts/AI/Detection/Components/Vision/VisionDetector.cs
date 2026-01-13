using Detection.Core;
using UnityEngine;

namespace Detection.Vision
{
    public class VisionDetector : MonoBehaviour, IDetector
    {
        [SerializeField] private VisualDetectionStats stats;
        [SerializeField] private Transform head;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private bool enableVerboseLogging = false;

        private readonly Collider[] _colliders = new Collider[32];
        private readonly DetectedObject[] _detectedCache = new DetectedObject[32];
        private int _detectedCount;
        
        public int UpdateDetector(DetectedObject[] detectedObjects)
        {
            if (enableDebugLogs)
                Debug.Log($"[VisionDetector] UpdateDetector called on {gameObject.name}");
            
            if (!head  || !stats)
            {
                if (enableDebugLogs)
                    Debug.LogWarning($"[VisionDetector] Missing head or stats on {gameObject.name}");
                return 0;
            }

            _detectedCount = 0;
            
            // Find all colliders in detection range
            int hits = Physics.OverlapSphereNonAlloc(
                head.position, 
                stats.DetectRange, 
                _colliders, 
                stats.BlockingLayerMask
            );
            
            if (enableDebugLogs)
                Debug.Log($"[VisionDetector] OverlapSphere hits: {hits} at position {head.position}");
            
            // Process each hit
            for (int i = 0; i < hits && _detectedCount < stats.TrackedTargetLimit; i++)
            {
                if (enableDebugLogs && enableVerboseLogging)
                    Debug.Log($"[VisionDetector] Processing hit {i}/{hits}, detected count: {_detectedCount}/{stats.TrackedTargetLimit}");

                Vector3 targetPos = _colliders[i].transform.position;
                
                // Check if target is within vision cone and visible
                if (!CanSeeTarget(targetPos, _colliders[i]))
                {
                    if (enableDebugLogs)
                        Debug.Log($"[VisionDetector] Can't see target: {_colliders[i].name}", _colliders[i].gameObject);
                    continue;
                }
                
                // Try to get IDetectable component
                Rigidbody rb = _colliders[i].attachedRigidbody;
                
                if (!(rb && rb.TryGetComponent(out IDetectable detectable)) && !_colliders[i].transform.root.TryGetComponent(out detectable))
                {
                    if (enableDebugLogs)
                        Debug.Log($"[VisionDetector] Object {_colliders[i].name} is not detectable");
                    continue;
                }
                
                // Check if already in our cache
                int existingIndex = -1;
                for (int j = 0; j < _detectedCount; j++)
                {
                    if (_detectedCache[j].Detectable == detectable)
                    {
                        existingIndex = j;
                        break;
                    }
                }
                
                if (existingIndex == -1)
                {
                    // New detection - add to cache
                    _detectedCache[_detectedCount] ??= new DetectedObject();
                    _detectedCache[_detectedCount].Detectable = detectable;
                    _detectedCache[_detectedCount].Time = 0f;
                    
                    // Copy to output array
                    detectedObjects[_detectedCount] = _detectedCache[_detectedCount];
                    
                    if (enableDebugLogs)
                    {
                        MonoBehaviour mb = detectable as MonoBehaviour;
                        Debug.Log($"[VisionDetector] NEW detection added: {(mb ? mb.gameObject.name : "Unknown")} at index {_detectedCount}");
                    }
                    
                    _detectedCount++;
                }
                else
                {
                    // Existing detection - copy to output
                    detectedObjects[existingIndex] = _detectedCache[existingIndex];
                    
                    if (enableDebugLogs && enableVerboseLogging)
                        Debug.Log($"[VisionDetector] Existing detection maintained at index {existingIndex}");
                }
            }
            
            if (enableDebugLogs)
                Debug.Log($"[VisionDetector] Total detected objects: {_detectedCount}");

            return _detectedCount;
        }

        private bool CanSeeTarget(Vector3 targetPosition, Collider target)
        {
            
            Vector3 forward = (targetPosition - head.position).normalized;
            
            // Check angle first (cheaper)
            float dotProduct = Vector3.Dot(head.forward, forward);
            if (dotProduct < stats.DetectAngle)
            {
                if (enableDebugLogs && enableVerboseLogging)
                    Debug.Log($"[VisionDetector] Target failed angle check. Dot: {dotProduct:F3}, Required: {stats.DetectAngle:F3}");
                return false;
            }
            
            // Then raycast
            bool hit = Physics.Raycast(
                head.position, 
                forward, 
                out RaycastHit hitInfo, 
                stats.DetectRange, 
                stats.BlockingLayerMask
            );
            
            if (enableDebugLogs && enableVerboseLogging && !hit) Debug.Log($"[VisionDetector] Raycast missed target at {targetPosition}");
            
            return hitInfo.collider == target;
        }

        #if UNITY_EDITOR
        #region Gizmos
        
        [Header("Gizmo Settings")]
        public DetectorGizmoSettings gizmoSettings = new();
        
        private void OnDrawGizmosSelected()
        {
            if (head == null || stats == null) return;

            Vector3 origin = head.position;
            float range = stats.DetectRange;
            float angleInDegrees = Mathf.Acos(stats.DetectAngle) * Mathf.Rad2Deg;

            // 1. Draw detection range sphere
            if (gizmoSettings.showDetectionRange)
            {
                Gizmos.color = gizmoSettings.detectionRangeColor;
                Gizmos.DrawWireSphere(origin, range);
            }

            // 2. Draw forward direction ray
            if (gizmoSettings.showForwardDirection)
            {
                Gizmos.color = gizmoSettings.forwardDirectionColor;
                Gizmos.DrawRay(origin, head.forward * range);
            }

            // 3. Draw vision cone edges
            if (gizmoSettings.showVisionCone)
            {
                Gizmos.color = gizmoSettings.visionConeColor;
                
                // Vertical cone edges
                Vector3 upEdge = head.rotation * Quaternion.Euler(angleInDegrees, 0, 0) * Vector3.forward * range;
                Vector3 downEdge = head.rotation * Quaternion.Euler(-angleInDegrees, 0, 0) * Vector3.forward * range;
                Gizmos.DrawRay(origin, upEdge);
                Gizmos.DrawRay(origin, downEdge);

                // Horizontal cone edges
                Vector3 rightEdge = head.rotation * Quaternion.Euler(0, angleInDegrees, 0) * Vector3.forward * range;
                Vector3 leftEdge = head.rotation * Quaternion.Euler(0, -angleInDegrees, 0) * Vector3.forward * range;
                Gizmos.DrawRay(origin, rightEdge);
                Gizmos.DrawRay(origin, leftEdge);
            }

            // 4. Draw diagonal cone edges
            if (gizmoSettings.showDiagonalEdges)
            {
                Gizmos.color = gizmoSettings.visionConeDiagonalColor;
                Vector3 topRightEdge = Quaternion.Euler(angleInDegrees * 0.7f, angleInDegrees * 0.7f, 0) * head.forward * range;
                Vector3 topLeftEdge = Quaternion.Euler(angleInDegrees * 0.7f, -angleInDegrees * 0.7f, 0) * head.forward * range;
                Vector3 bottomRightEdge = Quaternion.Euler(-angleInDegrees * 0.7f, angleInDegrees * 0.7f, 0) * head.forward * range;
                Vector3 bottomLeftEdge = Quaternion.Euler(-angleInDegrees * 0.7f, -angleInDegrees * 0.7f, 0) * head.forward * range;
                
                Gizmos.DrawRay(origin, topRightEdge);
                Gizmos.DrawRay(origin, topLeftEdge);
                Gizmos.DrawRay(origin, bottomRightEdge);
                Gizmos.DrawRay(origin, bottomLeftEdge);
            }

            // 5. Draw cone arcs
            if (gizmoSettings.showConeArcs)
            {
                DrawConeArc(origin, head.forward, head.up, angleInDegrees, range * 0.33f, gizmoSettings.coneArcNearColor);
                DrawConeArc(origin, head.forward, head.up, angleInDegrees, range * 0.66f, gizmoSettings.coneArcMidColor);
                DrawConeArc(origin, head.forward, head.up, angleInDegrees, range, gizmoSettings.coneArcFarColor);
            }

            // 6. Show detected objects
            if (gizmoSettings.showDetectables && Application.isPlaying)
            {
                for (int i = 0; i < _detectedCount; i++)
                {
                    if (_detectedCache[i]?.Detectable == null) continue;
                    
                    MonoBehaviour mb = _detectedCache[i].Detectable as MonoBehaviour;
                    if (mb == null) continue;
                    
                    Vector3 targetPos = mb.transform.position;
                    bool canSee = CanSeeTarget(targetPos, mb.GetComponent<Collider>());
                    
                    Color lineColor = canSee 
                        ? gizmoSettings.detectableStartColor 
                        : gizmoSettings.detectableBlockedColor;
                    
                    Gizmos.color = lineColor;
                    Gizmos.DrawLine(origin, targetPos);
                    
                    // Draw sphere at detectable
                    Color sphereColor = lineColor;
                    sphereColor.a *= 0.6f;
                    Gizmos.color = sphereColor;
                    Gizmos.DrawSphere(targetPos, 0.3f);
                }
            }

            // 7. Draw distance markers
            if (gizmoSettings.showDistanceMarkers)
            {
                Gizmos.color = gizmoSettings.distanceMarkerColor;
                for (int i = 1; i <= 3; i++)
                {
                    float distance = range * (i / 3f);
                    Gizmos.DrawWireSphere(origin, distance);
                }
            }
        }

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
        
        [System.Serializable]
        public class DetectorGizmoSettings
        {
            [Header("Gizmo Toggles")]
            public bool showDetectionRange = true;
            public bool showForwardDirection = true;
            public bool showVisionCone = true;
            public bool showDiagonalEdges = true;
            public bool showConeArcs = true;
            public bool showDistanceMarkers = true;
            public bool showDetectables = true;
            public bool showCurrentTarget = true;
            public bool showTargetLine = true;
        
            [Header("Colors - Detection Range")]
            public Color detectionRangeColor = new Color(0.3f, 0.6f, 1f, 0.15f);
        
            [Header("Colors - Forward Direction")]
            public Color forwardDirectionColor = new Color(1f, 1f, 0f, 0.9f);
        
            [Header("Colors - Vision Cone")]
            public Color visionConeColor = new Color(0f, 1f, 0.5f, 0.7f);
            public Color visionConeDiagonalColor = new Color(0f, 1f, 0.5f, 0.4f);
            public Color coneArcNearColor = new Color(0f, 1f, 0.5f, 0.3f);
            public Color coneArcMidColor = new Color(0f, 1f, 0.5f, 0.3f);
            public Color coneArcFarColor = new Color(0f, 1f, 0.5f, 0.5f);
        
            [Header("Colors - Distance Markers")]
            public Color distanceMarkerColor = new Color(1f, 1f, 1f, 0.3f);
        
            [Header("Colors - Detectables")]
            public Color detectableStartColor = Color.white;
            public Color detectableEndColor = Color.red;
            public Color detectableBlockedColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        
            [Header("Colors - Current Target")]
            public Color targetVisibleColor = new Color(0f, 1f, 0f, 0.8f);
            public Color targetLostColor = new Color(1f, 0.5f, 0f, 0.6f);
            public Color targetLockedColor = new Color(1f, 0f, 0f, 0.8f);
            public Color chaseTimerStartColor = Color.red;
            public Color chaseTimerEndColor = Color.yellow;
        }
        #endregion
        #endif
    }
}