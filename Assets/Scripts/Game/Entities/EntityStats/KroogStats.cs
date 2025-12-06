using UnityEngine;

namespace Game.Entities.EntityStats
{
    [CreateAssetMenu(fileName = "KroogStats", menuName = "Scriptable Objects/KroogStats")]
    public class KroogStats : ScriptableObject
    {
        [SerializeField] private float kroogHealth;
        [SerializeField] private float kroogDamage;
        [SerializeField] private float kroogSpeed;
        [SerializeField] private float detectRange;
        [SerializeField] private float attackRange;
        [SerializeField] private GameObject[] targets;
        [SerializeField] private Vector2 attackForce;
        [SerializeField] private int mitosisAmount;
        [SerializeField] private float mitosisForce;
        
        public float KroogHealth => kroogHealth;
        public float KroogDamage => kroogDamage;
        public float KroogSpeed => kroogSpeed;
        public float DetectRange => detectRange;

        public GameObject[] Targets
        {
            get => targets;
            set => targets = value;
        }
        public Vector2 AttackForce => attackForce;
        public int MitosisAmount => mitosisAmount;
        public float MitosisForce => mitosisForce;
    }
}
