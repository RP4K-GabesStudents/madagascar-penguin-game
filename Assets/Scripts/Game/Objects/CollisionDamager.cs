using Unity.Netcode;
using UnityEngine;

namespace Game.Objects
{
    public class CollisionDamager : MonoBehaviour
    {
        
        [SerializeField] private CollisionDamagerStats collisionDamagerStats;

        private void OnCollisionEnter(Collision other)
        {
            // Only the authoritative peer sends the hit (or local play with no
            // active server, where we just apply it directly).
            var nm = NetworkManager.Singleton;
            bool networked = nm != null && nm.IsListening;
            if (networked && !nm.IsServer) return;

            Rigidbody rb = other.rigidbody;
            if (rb && rb.TryGetComponent(out IDamageable target) ||
                other.transform.TryGetComponent(out target))
            {
                float damage = collisionDamagerStats.GetDamageFromSpeed(other.relativeVelocity.magnitude);
                target.ApplyNetworkedDamage(damage, other.impulse);

                Debug.DrawRay(transform.position, other.relativeVelocity, Color.red, 5);
                Debug.DrawRay(transform.position, other.impulse, Color.green, 5);
            }
        }
    }
}