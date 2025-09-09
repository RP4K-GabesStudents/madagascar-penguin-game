using Game.Objects;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats.Penguin
{
    [CreateAssetMenu(fileName = "LaserEyeCapabilityStats", menuName = "Characters/CapabilityStats/LaserEyeCapabilityStats")]
    public class LaserEyeCapabilityStats : Characters.CapabilityStats
    {
       // public override Type GetCapabilityType() => typeof(CrouchCapability);
       [SerializeField] private Laser projectile;
       [SerializeField] private ParticleSystem particleSystem;
       [SerializeField, Min(1)] private int numProjectiles = 1;
       [SerializeField, Range(0,90)] private float inaccuracy = 0;

        public Laser Projectile => projectile;
        public ParticleSystem ParticleSystem => particleSystem;
        public int NumProjectiles => numProjectiles;
        public float Inaccuracy => inaccuracy;
    }
}