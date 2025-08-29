using UnityEngine;

namespace Game.AbilitySystem.Explosion
{
    [CreateAssetMenu(fileName = "ExplosionStats", menuName = "Scriptable Objects/ExplosionStats")]
    public class ExplosionStats : ScriptableObject
    {
        [SerializeField] private float minDamage = 5f;
        [SerializeField] private float maxDamage = 1000f;
        [SerializeField] private float explosionRadius = 8f;
        [SerializeField] private float explosionTime = 4f;
        [SerializeField] private float minExplosionForce = 7;
        [SerializeField] private float maxExplosionForce = 20f;
        [SerializeField] private AnimationCurve dropOff;
        [SerializeField] private AudioClip audio;
        
        public float ExplosionDamage(float percent) => Mathf.InverseLerp(minDamage, maxDamage, dropOff.Evaluate(percent));
        public float ExplosionRadius => explosionRadius;
        public float ExplosionTime => explosionTime;
        public float ExplosionForce(float percent) => Mathf.InverseLerp(minExplosionForce, minExplosionForce, dropOff.Evaluate(percent));
        public AudioClip Audio => audio;
    }
}
