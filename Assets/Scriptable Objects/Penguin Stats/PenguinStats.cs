using UnityEngine;

namespace Scriptable_Objects.Penguin_Stats
{
    [CreateAssetMenu(fileName = "PenguinStats", menuName = "Scriptable Objects/PenguinStats")]
    public class PenguinStats : ScriptableObject
    {
        [SerializeField] private float hp = 0;
        [SerializeField] private float speed = 0;
        [SerializeField] private float speedLimit = 0;
        [SerializeField] private float damage = 0;
        [SerializeField] private float jumpPower = 0;
        [SerializeField] private float slideSpeed = 0;
        [SerializeField] private float attackCooldown = 0;
        [SerializeField] private float jumpCooldown = 0; 
        [SerializeField] private float slideCooldown = 0;
        [SerializeField] private float sprintSpeed = 0;
        [SerializeField] private float attackRadius;
        [SerializeField] private float maxAttackDist;
        [SerializeField] private float knockbackPower;
        [SerializeField] private bool canShootLaser;
        


        public float Hp => hp;
        public float Speed { get => speed; set => speed = value; }
        public float Damage => damage;
        public float JumpPower => jumpPower;
        public float SlideSpeed => slideSpeed;
        public float AttackCooldown => attackCooldown;
        public float JumpCooldown => jumpCooldown;
        public float SlideCooldown => slideCooldown;
        public float SpeedLimit => speedLimit;
        public float SprintSpeed => sprintSpeed;
        public float AttackRadius => attackRadius;
        public float MaxAttackDist => maxAttackDist;
        public float KnockbackPower => knockbackPower;
        public bool CanShootLaser { get => canShootLaser; set => canShootLaser = value; }
    }
}
