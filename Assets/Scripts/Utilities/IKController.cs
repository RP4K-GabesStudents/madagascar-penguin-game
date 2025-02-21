using System;
using Managers;
using UnityEngine;

namespace Utilities
{
    [RequireComponent(typeof(Animator))]

    public class IKController : MonoBehaviour
    {
        [SerializeField] private float footDist;
        [SerializeField] private float footRadius; 
        [SerializeField] private Transform leftFoot;    
        [SerializeField] private Transform rightFoot;    
        private Animator _animator;
        private Vector3 _leftFootPosition;
        private Vector3 _rightFootPosition;
        private Quaternion _leftFootRotation;
        private Quaternion _rightFootRotation;
        public bool IsGrounded {get; private set; }
        public event Action OnGroundExit;
        public event Action OnGroundEnter;
        void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            HandleFeet();
        }

        void OnAnimatorIK(int layerIndex)
        {
            _animator.SetIKPosition(AvatarIKGoal.LeftFoot, _leftFootPosition);
            _animator.SetIKRotation(AvatarIKGoal.LeftFoot, _leftFootRotation);
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
        
            _animator.SetIKPosition(AvatarIKGoal.RightFoot, _rightFootPosition);
            _animator.SetIKRotation(AvatarIKGoal.RightFoot, _rightFootRotation);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
        }

        private void HandleFeet()
        {
            bool handleLeftFoot = Detect(leftFoot, out _leftFootPosition, out _leftFootRotation);
            bool handleRightFoot = Detect(rightFoot, out _rightFootPosition, out _rightFootRotation);
            bool isGrounded = handleLeftFoot || handleRightFoot;
            if (isGrounded != IsGrounded)
            {
                if (isGrounded)
                {
                    OnGroundEnter?.Invoke();
                }
                else
                {
                    OnGroundExit?.Invoke();
                }
            }
            IsGrounded = isGrounded;
        }

        private bool Detect(Transform target, out Vector3 footPos, out Quaternion footRot)
        {
            if (Physics.SphereCast(target.position,footRadius,  Vector3.down, out RaycastHit floor, footDist, StaticUtilities.GroundLayers))
            {
                footPos = floor.point;
                footRot = Quaternion.LookRotation(Vector3.up, floor.normal);
                return true;
            }
            footPos = Vector3.down * footDist + target.position;
            footRot = Quaternion.identity;
            return false;
        }

#if UNITY_EDITOR
    
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(leftFoot.position, footRadius);
            Gizmos.DrawWireSphere(rightFoot.position, footRadius);
            Gizmos.DrawLine(leftFoot.position, leftFoot.position + Vector3.down * footDist);
            Gizmos.DrawLine(rightFoot.position, rightFoot.position + Vector3.down * footDist);
            if (leftFoot)
            {
                Gizmos.color = Physics.SphereCast(leftFoot.position,footRadius,  Vector3.down, out RaycastHit left, footDist, StaticUtilities.GroundLayers) ? Color.green : Color.red;
                Gizmos.DrawWireSphere(left.point, footRadius);
            }

            if (rightFoot)
            {
                Gizmos.color = Physics.SphereCast(rightFoot.position,footRadius,  Vector3.down, out RaycastHit right, footDist, StaticUtilities.GroundLayers) ? Color.green : Color.red;
                Gizmos.DrawWireSphere(right.point, footRadius);
            }
        
        }
#endif
    }
}


