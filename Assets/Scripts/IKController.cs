using System;
using DefaultNamespace;
using Unity.Mathematics;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]

public class IKController : MonoBehaviour
{
    [SerializeField] private float footDist;
    [SerializeField] private float footRadius; 
    [SerializeField] private Transform leftFoot;    
    [SerializeField] private Transform rightFoot;    
    private Animator _animator;
    public bool IsGrounded {get; private set; }
    public event Action OnGroundExit;
    public event Action OnGroundEnter;
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    void OnAnimatorIK(int layerIndex)
    {
        HandleFeet();
    }
    private bool HandleFoot(AvatarIKGoal foot, Transform target)
    {
        bool ground = Detect(target, out Vector3 footPos, out Quaternion footRot);
        _animator.SetIKPosition(foot, footPos);
        _animator.SetIKRotation(foot, footRot);
        return ground;
    }
    private void HandleFeet()
    {
        bool handleLeftFoot = HandleFoot(AvatarIKGoal.LeftFoot, leftFoot);
        bool handleRightFoot = HandleFoot(AvatarIKGoal.RightFoot, rightFoot);
        bool isGrounded = handleLeftFoot || handleRightFoot;
        if (isGrounded != IsGrounded)
        {
            IsGrounded = isGrounded;
            if (isGrounded)
            {
                OnGroundEnter?.Invoke();
            }
            else
            {
                OnGroundExit?.Invoke();
            }
        }
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


