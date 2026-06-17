using Game.Objects;
using Unity.Netcode;
using UnityEngine;

namespace AI.Characters
{
    public class GenericEnemy : NetworkBehaviour, IDamageable
    {
        #region Health
        [field: SerializeField] public float Health { get; set; }
        [field: SerializeField] public float DamageRes { get; private set; }

        public virtual void Die(Vector3 force)
        {

        }

        public virtual void OnHurt(float amount, Vector3 force)
        {

        }

        // Required per-class IDamageable stub. Verbatim across implementers;
        // must live on the concrete NetworkBehaviour because [Rpc] needs one.
        [Rpc(SendTo.Server)]
        public void TakeDamageRpc(float damage, Vector3 force) =>
            ((IDamageable)this).TakeDamageLocal(damage, force);
        #endregion
    }
}