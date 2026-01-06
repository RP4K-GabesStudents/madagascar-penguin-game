using System;
using System.Collections.Generic;
using Detection.Core;
using UnityEngine;

namespace Detection.Controllers
{
    public class DetectionController : MonoBehaviour
    {
        // Returns the highest detection percentage (0-1) of any tracked object
        public float DetectionPercent
        {
            get
            {
                float maxPercent = 0f;
                foreach (var kvp in _trackedObjects)
                {
                    float percent = Mathf.Clamp01(kvp.Value.Time / stats.DetectionTime);
                    maxPercent = Mathf.Max(maxPercent, percent);
                }
                return maxPercent;
            }
        }

        private IDetector[] _detectors;
        
        public event Action<IDetectable> OnDetectionGained;
        public event Action<IDetectable> OnDetectionLost;

        [SerializeField] private DetectionControllerStats stats;

        
        private DetectedObject[] _detectionBuffer;
        
        private readonly Dictionary<IDetectable, DetectedObject> _trackedObjects = new();
        private readonly HashSet<IDetectable> _currentFrameDetections = new();
        private readonly List<IDetectable> _toRemove = new();
        
        private void Awake()
        {
            _detectors = GetComponentsInChildren<IDetector>();
            _detectionBuffer = new DetectedObject[ stats.MaxDetectionsPerDetector];
        }
        
        private void Update()
        {
            float dt = Time.deltaTime;
            
            // Step 1: Gather all currently detected objects using fixed-size buffer
            _currentFrameDetections.Clear();
            foreach (var detector in _detectors)
            {
                int hitCount = detector.UpdateDetector(_detectionBuffer);
                
                // Clamp to buffer size - detector is responsible for prioritizing
                int count = Mathf.Min(hitCount, _detectionBuffer.Length);
                
                for (int i = 0; i < count; i++)
                {
                    _currentFrameDetections.Add(_detectionBuffer[i].Detectable);
                }
            }
            
            // Step 2: Update detection times for currently detected objects
            foreach (var detectable in _currentFrameDetections)
            {
                if (!_trackedObjects.TryGetValue(detectable, out var detectedObject))
                {
                    detectedObject = new DetectedObject 
                    { 
                        Time = 0f, 
                        Detectable = detectable 
                    };
                    _trackedObjects[detectable] = detectedObject;
                }
                
                float previousTime = detectedObject.Time;
                detectedObject.Time = Mathf.Min(detectedObject.Time + dt, stats.DetectionTime);
                
                // Check if we just crossed the detection threshold
                if (previousTime < stats.DetectionTime && detectedObject.Time >= stats.DetectionTime)
                {
                    detectable.OnDetectedBy(this);
                    OnDetectionGained?.Invoke(detectable);
                }
            }
            
            // Step 3: Decrease time for objects no longer detected
            _toRemove.Clear();
            foreach (var kvp in _trackedObjects)
            {
                if (!_currentFrameDetections.Contains(kvp.Key))
                {
                    var detectedObject = kvp.Value;
                    float previousTime = detectedObject.Time;
                    detectedObject.Time = Mathf.Max(detectedObject.Time - dt, 0f);
                    
                    // Check if we just dropped below the detection threshold
                    if (previousTime >= stats.DetectionTime && detectedObject.Time < stats.DetectionTime)
                    {
                        kvp.Key.OnDetectionLost(this);
                        OnDetectionLost?.Invoke(kvp.Key);
                    }
                    
                    if (detectedObject.Time <= 0f)
                    {
                        _toRemove.Add(kvp.Key);
                    }
                }
            }
            
            // Step 4: Clean up
            foreach (var detectable in _toRemove)
            {
                _trackedObjects.Remove(detectable);
            }
        }
    }
}