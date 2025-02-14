using UnityEngine;

namespace DefaultNamespace
{
    public static class StaticUtilities
    {
        public static readonly int ForwardAnimID = Animator.StringToHash("Forward");
        public static readonly int IdleStateAnimID = Animator.StringToHash("IdleState");
        public static readonly int AttackAnimID = Animator.StringToHash("Attack");
        public static readonly int InteractAnimID = Animator.StringToHash("Interact");
        public static readonly int JumpingAnimID = Animator.StringToHash("Jumping");
        public static readonly int SlidingAnimID = Animator.StringToHash("Sliding");
        
        public static readonly int DefaultLayer = 1 << LayerMask.NameToLayer("Default");
        public static readonly int GroundLayers = DefaultLayer;
        
    }
}