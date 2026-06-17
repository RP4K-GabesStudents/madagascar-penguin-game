using Unity.Netcode;
using UnityEngine;

namespace Game.Objects
{
    /// <summary>
    /// Anything that can be hurt. The damage MATH lives here as default
    /// interface methods, so all 11+ implementers share one definition of what
    /// "taking damage" means. What each implementer cannot inherit is the
    /// networking primitive: an [Rpc] must live on the concrete NetworkBehaviour
    /// for NGO's codegen to see it. So every implementer supplies exactly one
    /// tiny stub (TakeDamageRpc) and the routing below ties it together.
    ///
    /// Damage authority model (project choice: trust the attacker, prioritise
    /// the attacker's feel over anti-cheat):
    ///   attacker calls victim.ApplyNetworkedDamage(dmg, force)
    ///     -> that sends the victim's TakeDamageRpc with SendTo.Server
    ///     -> server runs TakeDamage(...) (the default logic below) locally
    ///     -> whatever state TakeDamage mutates replicates by its own means
    ///        (a NetworkVariable health for a player; a Despawn for a potion).
    /// The attacker never re-simulates the victim; it just makes the claim.
    /// </summary>
    public interface IDamageable
    {
        float Health { get; set; }
        float DamageRes { get; }

        void Die(Vector3 force);
        void OnHurt(float amount, Vector3 force);

        /// <summary>
        /// The one per-class primitive. Every implementer pastes this verbatim
        /// (it cannot live in the interface because [Rpc] needs a concrete
        /// NetworkBehaviour). The body just runs the shared TakeDamage logic,
        /// which now executes on the server because of SendTo.Server.
        ///
        ///     [Rpc(SendTo.Server)]
        ///     public void TakeDamageRpc(float damage, Vector3 force) =>
        ///         ((IDamageable)this).TakeDamageLocal(damage, force);
        /// </summary>
        void TakeDamageRpc(float damage, Vector3 force);

        /// <summary>
        /// Entry point for attackers. Resolves this victim's NetworkObject and
        /// fires its server Rpc. Works whether the caller is a client or the
        /// server (the Rpc executes locally if invoked on the server).
        /// </summary>
        sealed void ApplyNetworkedDamage(float damage, Vector3 force)
        {
            if (this is not NetworkBehaviour) // can't network a non-networked victim
            {
                // Fallback: purely local damageable (e.g. a singleplayer-only
                // object). Apply directly; nothing to replicate.
                TakeDamageLocal(damage, force);
                return;
            }

            TakeDamageRpc(damage, force);
        }

        /// <summary>
        /// The actual damage math, unchanged from before. Renamed from the old
        /// default TakeDamage so the networked path (ApplyNetworkedDamage) is
        /// the obvious public door and this is the thing the server runs.
        /// </summary>
        sealed void TakeDamageLocal(float damage, Vector3 force)
        {
            damage *= 1 - DamageRes;
            if (damage <= 0) return;

            OnHurt(damage, force);
            Health -= damage;
            if (Health <= 0) Die(force);
        }
    }
}