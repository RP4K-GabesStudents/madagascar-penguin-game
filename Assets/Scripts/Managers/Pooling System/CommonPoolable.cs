using UnityEngine;

namespace Managers.Pooling_System
{
    public class CommonPoolable : MonoBehaviour, IPoolable
    {
        [SerializeField] private float lifeTime;
        private float _curTime;

        // Stays at root (never parented) so Update/timer always runs even if the
        // surface it stuck to gets deactivated. Reproduces SetParent(true) behaviour
        // by snapshotting the pose relative to the target and rebuilding it each frame.
        private Transform _follow;
        private Vector3 _localPos;
        private Quaternion _localRot;

        private void OnEnable()
        {
            _curTime = lifeTime;
        }

        private void OnDisable()
        {
            _follow = null;
        }

        // Attach to a surface while keeping current world pose. Scale-correct because
        // it uses the target's world matrix (TransformPoint), like SetParent(true).
        public void Follow(Transform target)
        {
            _follow = target;
            if (target == null) return;
            _localPos = target.InverseTransformPoint(transform.position);
            _localRot = Quaternion.Inverse(target.rotation) * transform.rotation;
        }

        private void LateUpdate()
        {
            if (_follow == null) return;
            // Target destroyed: stop following, stay where we are.
            if (!_follow) { _follow = null; return; }

            transform.SetPositionAndRotation(_follow.TransformPoint(_localPos),_follow.rotation * _localRot);
        }

        private void Update()
        {
            _curTime -= Time.deltaTime;
            if (_curTime <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        public void ForceDespawn() { }
        public void Spawn(ulong spawnID) { }
    }
}