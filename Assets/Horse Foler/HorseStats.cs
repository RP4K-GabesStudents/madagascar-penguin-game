using UnityEngine;
using UnityEngine.Serialization;

namespace Horse_Foler
{
    [CreateAssetMenu(fileName = "HorseStats", menuName = "Scriptable Objects/HorseStats")]
    public class HorseStats : ScriptableObject
    {
        [SerializeField] private float health;
        [SerializeField] private float damage;
        [SerializeField] private float attackSpeed;
        [SerializeField] private float speed;
        [SerializeField] private float attackRange;
        [SerializeField] private float attackRadius;
        [SerializeField] private Vector3 attackForce;
        
        public float AttackRange => attackRange;
        public float AttackSpeed => attackSpeed;
        public float Speed => speed;
        public float Damage => damage;
        public float Health => health;
        public float AttackArea => attackRadius;

        public Vector3 AttackForce => attackForce;
    }
}
