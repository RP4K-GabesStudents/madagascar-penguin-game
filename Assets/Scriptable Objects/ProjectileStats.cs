using UnityEngine;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "ProjectileStats", menuName = "Scriptable Objects/ProjectileStats")]
    public class ProjectileStats : ScriptableObject
    {
        [SerializeField] private float damage;
        [SerializeField] private float speed;
        [SerializeField] private float lifetime;
        [SerializeField] private float abilityTime;
        [SerializeField] private float maxSpeed;
        [SerializeField] private int amountFired;
        [SerializeField] private ParticleSystem laserSpark;
        
        public float Damage
        {
            get => damage;
            set => damage = value;
        }

        public float Speed => speed;
        public float Lifetime => lifetime;
        public float AbilityTime => abilityTime;
        public float MaxSpeed => maxSpeed;
        public int AmountFired => amountFired;
        public ParticleSystem LaserSpark => laserSpark;
    }
}
