using UnityEngine;

namespace Managers
{
    public static class StaticUtilities
    {
        //animations
        public static readonly int ForwardAnimID = Animator.StringToHash("Forward");
        public static readonly int IdleStateAnimID = Animator.StringToHash("IdleState");
        public static readonly int AttackAnimID = Animator.StringToHash("Attack");
        public static readonly int InteractAnimID = Animator.StringToHash("Interact");
        public static readonly int JumpingAnimID = Animator.StringToHash("Jumping");
        public static readonly int SlidingAnimID = Animator.StringToHash("Sliding");
        
        //layers
        public static readonly int DefaultLayer = 1 << LayerMask.NameToLayer("Default");
        
        public static readonly int EnemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        public static readonly int PlayerLayer = 1 << LayerMask.NameToLayer("Player");
        public static readonly int DestructableLayer = 1 << LayerMask.NameToLayer("Destructable");
        public static readonly int PhysicsLayer = 1 << LayerMask.NameToLayer("PhysicsObjects");
        public static readonly int InteractableLayer = 1 << LayerMask.NameToLayer("InteractableLayer");
        
        public static readonly int GroundLayers = DefaultLayer;
        public static readonly int AttackableLayers = EnemyLayer | PlayerLayer | DestructableLayer | DefaultLayer | PhysicsLayer;
        public static readonly int EnemyAttackLayers = EnemyLayer | DestructableLayer | DefaultLayer;
        public static readonly int InteractableLayers = PhysicsLayer | InteractableLayer;

    }
}