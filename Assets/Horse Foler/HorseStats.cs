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
        [SerializeField] private Vector2 attackForce;
        [SerializeField] private float detectionRadius;
        [SerializeField] private GameObject[] targets;
        
        public float AttackRange => attackRange;
        public float AttackSpeed => attackSpeed;
        public float Speed => speed;
        public float Damage => damage;
        public float Health => health;
        public float AttackArea => attackRadius;

        public Vector2 AttackForce => attackForce;
        public float DetectionRadius => detectionRadius;
        public GameObject[] Targets
        {
            get => targets;
            set => targets = value;
        }
    }
}
