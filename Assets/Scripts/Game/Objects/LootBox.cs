using Game.Objects;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;

namespace Objects
{
    /// <summary>
    /// A destructible loot container. Networked so the box is a shared world
    /// object: the killing hit is applied on the server and loot spawns once,
    /// authoritatively, rather than per-peer.
    ///
    /// (If loot boxes are actually local/singleplayer-only, this could be a
    /// plain MonoBehaviour relying on IDamageable's non-NetworkBehaviour
    /// fallback, but then TakeDamageRpc has no home and the interface's
    /// required-stub contract wouldn't be satisfiable. Kept networked.)
    /// </summary>
    public class LootBox : NetworkBehaviour, IDamageable
    {
        [SerializeField] private LootTable lootTable;

        [field: SerializeField] public float Health { get; set; }
        public float DamageRes => 0;

        // Required per-class IDamageable stub. Verbatim across implementers.
        [Rpc(SendTo.Server)]
        public void TakeDamageRpc(float damage, Vector3 force) =>
            ((IDamageable)this).TakeDamageLocal(damage, force);

        public void DropLoot()
        {
            Debug.Log("I dropped my loot");
            lootTable.Spawn(transform.position, 3, 0.1f);
        }

        // Runs on the server (via TakeDamageLocal). Loot spawning should be
        // server-side; ensure LootTable.Spawn spawns NetworkObjects.
        public void Die(Vector3 force)
        {
            DropLoot();
            if (IsSpawned) NetworkObject.Despawn();
        }

        public void OnHurt(float amount, Vector3 force) { }
    }
}