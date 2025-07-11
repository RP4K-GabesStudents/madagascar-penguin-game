using UnityEngine;

namespace Interfaces
{
    public interface IDamageable
    {
        public void TakeDamage(float damage, Vector3 force)
        {
            damage -= DamageRes;
            if (damage <= 0)
            {
                return;
            }
            OnHurt(damage, force);
            Health -= damage;
            if (Health <= 0)
            {
                Die(force);
            }
        }
        public void Die(Vector3 force);
        public void OnHurt(float amount, Vector3 force);
        public float Health { get; set; }
        public float DamageRes { get; }
    }
}