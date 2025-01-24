using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "PenguinStats", menuName = "Scriptable Objects/PenguinStats")]
    public class PenguinStats : ScriptableObject
    {
        [SerializeField] private float hp = 0;
        [SerializeField] private float speed = 0;
        [SerializeField] private float damage = 0;
        [SerializeField] private float jumpPower = 0;
        [SerializeField] private float slideSpeed = 0;
        [SerializeField] private float attackCooldown = 0;
        [SerializeField] private float jumpCooldown = 0; 
        [SerializeField] private float slideCooldown = 0;

        public float Hp => hp;
        public float Speed => speed;
        public float Damage => damage;
        public float JumpPower => jumpPower;
        public float SlideSpeed => slideSpeed;
        public float AttackCooldown => attackCooldown;
        public float JumpCooldown => jumpCooldown;
        public float SlideCooldown => slideCooldown;
    }
}
