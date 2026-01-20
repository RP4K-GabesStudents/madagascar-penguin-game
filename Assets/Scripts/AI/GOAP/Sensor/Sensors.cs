using System;
using UnityEngine;
using Utilities;

namespace AI.GOAP.Sensor
{
    [RequireComponent(typeof(SphereCollider))]
    public class Sensors : MonoBehaviour
    {
        private SensorStats _stats;
        private SphereCollider _detectRange;
        private GameObject _target;
        private Vector3 _lastKnownPosition;
        private CountdownTimer _timer;
        
        public event Action OnTargetChanged = delegate { };
        public Vector3 TargetPosition => _target ? _target.transform.position : Vector3.zero;
        public bool IsTargetInRange => TargetPosition != Vector3.zero;

        private void Awake()
        {
            _detectRange = GetComponent<SphereCollider>();
            _detectRange.isTrigger = true;
            _detectRange.radius = _stats.DetectionRadius;
        }

        private void Start()
        {
            _timer = new CountdownTimer(_stats.TimerInterval);
            _timer.OnTimerStop += () => UpdateTargetPosition(_target.OrNull());
            _timer.Start();
        }

        private void Update()
        {
            _timer.Tick(Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsTargetInRange ? Color.green : Color.red;
            Gizmos.DrawWireSphere(TargetPosition, _stats.DetectionRadius);
        }

        private void UpdateTargetPosition(GameObject target = null)
        {
            _target = target;
            if (IsTargetInRange && (_lastKnownPosition != TargetPosition || _lastKnownPosition != Vector3.zero))
            {
                _lastKnownPosition = TargetPosition;
                OnTargetChanged.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            UpdateTargetPosition(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            UpdateTargetPosition();
        }
    }
}
