using UnityEngine;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "ProjectileStats", menuName = "Scriptable Objects/ProjectileStats")]
    public class ProjectileStats : ScriptableObject
    {
        [SerializeField] private float damage;
        [SerializeField] private float speed;
        [SerializeField] private float lifetime;
        [SerializeField] private float laserAbilityTime;
        [SerializeField] private float maxSpeed;
        
        public float Damage => damage;
        public float Speed => speed;
        public float Lifetime => lifetime;
        public float LaserAbilityTime => laserAbilityTime;
        public float MaxSpeed => maxSpeed;
    }
}
