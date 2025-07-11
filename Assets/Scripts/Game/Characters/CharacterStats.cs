using UnityEngine;

namespace Game.Characters
{
    [CreateAssetMenu(fileName = "GenericCharacter", menuName = "Characters/GenericCharacter")]
    public class CharacterStats : ScriptableObject
    {
        [Header("Health")]
        [SerializeField] private float hp = 0;
        
        
        [Header("Movement")]
        [SerializeField] private float speed = 0;
        [SerializeField] private float sprintSpeed = 0;
        [SerializeField] private float speedLimit = 0;
        [Space]
        [SerializeField] private float jumpPower = 0;
        [SerializeField] private float jumpCooldown = 0; 
        
        [SerializeField] private float attackRadius;
        [SerializeField] private float maxAttackDist;
        


        public float Hp => hp;
        
        public float Speed => speed;
        public float SpeedLimit => speedLimit;
        public float SprintSpeed => sprintSpeed;
        public float JumpPower => jumpPower;
        public float JumpCooldown => jumpCooldown;
        
   
        
        public float AttackRadius => attackRadius;
        public float MaxAttackDist => maxAttackDist;
        public float KnockbackPower => knockbackPower;
        public bool CanShootLaser { get => canShootLaser; set => canShootLaser = value; }
    }
}