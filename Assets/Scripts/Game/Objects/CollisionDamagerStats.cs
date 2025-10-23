using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Objects
{
    [CreateAssetMenu(fileName = "CollisionDamagerStats", menuName = "Scriptable Objects/CollisionDamagerStats")]
    public class CollisionDamagerStats : ScriptableObject
    {
        [SerializeField] private float minDamage;
        [SerializeField] private float maxDamage;
        [SerializeField] private float minForce;
        [SerializeField] private float maxForce;
        [SerializeField] private AnimationCurve damageCurve;

        public float GetDamageFromSpeed(float speed)
        {
            if (speed < minForce) return 0;
            return Mathf.Min(Mathf.Lerp(minDamage, maxDamage, damageCurve.Evaluate(speed / maxForce)), maxDamage);
        }
    }
}
