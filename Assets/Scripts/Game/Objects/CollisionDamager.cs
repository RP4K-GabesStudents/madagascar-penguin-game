using UnityEngine;

namespace Game.Objects
{
    public class CollisionDamager : MonoBehaviour
    {
        [SerializeField] private CollisionDamagerStats collisionDamagerStats;

        private void OnCollisionEnter(Collision other)
        {
            Rigidbody rb = other.rigidbody;
            if (rb && rb.TryGetComponent(out IDamageable target) || other.transform.TryGetComponent(out target))
            {
                target.TakeDamage(collisionDamagerStats.GetDamageFromSpeed(other.relativeVelocity.magnitude), other.impulse);
                Debug.DrawRay(transform.position, other.relativeVelocity, Color.red, 5);
                Debug.DrawRay(transform.position, other.impulse, Color.green, 5);
                Debug.Log("I did damage: " + collisionDamagerStats.GetDamageFromSpeed(other.relativeVelocity.magnitude));
            }
        }
    }
}
