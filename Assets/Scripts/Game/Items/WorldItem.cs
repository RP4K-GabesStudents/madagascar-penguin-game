using Game.Characters.World;
using Game.Objects;
using InventorySystem.Core;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;

namespace Game.InventorySystem
{
    /// <summary>
    /// The world (on-the-ground) half of an item, and the replacement for the
    /// deleted Item.cs's interaction/hover responsibilities. Implements:
    ///   IInteractable - so InteractionCapability can hover/highlight/E it, and
    ///   IWorldItem     - so InventoryTransfer.TryPickup can absorb it as data.
    ///
    /// Stats now live in the ItemStats SO (resolved by id everywhere else), but
    /// the world object still references its own ItemStats directly so a loose
    /// prefab in a scene knows what it is without a registry lookup.
    ///
    /// Hover info is a SEPARATE SO from ItemStats: in the new system ItemStats
    /// extends ScriptableObject, not HoverInfoStats, so the tooltip text is its
    /// own asset assigned here. (In the old Item.cs these were the same object
    /// via inheritance; that chain is gone.)
    ///
    /// This is NOT the held behaviour. The same prefab also carries an
    /// IHeldItem (e.g. Potion, GenericWeapon) which only wakes up once the
    /// inventory spawns and parents the object into the mouth.
    ///
    /// Held vs world: this same prefab is spawned BOTH as a loose world pickup
    /// and as the equipped item in a player's mouth. While equipped it must not
    /// be pickable, or anyone (including the owner) could re-absorb it and
    /// duplicate it. IsHeld is the authoritative gate: the server sets it true
    /// on equip via SetHeld, and it defaults false so a dropped/loose item is
    /// pickable. It's a NetworkVariable so remote clients also see the held
    /// state and suppress hover/highlight locally.
    /// </summary>
    [SelectionBase, RequireComponent(typeof(Highlight))]
    public class WorldItem : NetworkBehaviour, IInteractable, IWorldItem
    {
        [SerializeField] private ItemStats itemStats;
        [SerializeField] private HoverInfoStats hoverInfo;
        [SerializeField, Min(1)] private int count = 1;

        // Server-writable; replicated so clients gate hover correctly too.
        private readonly NetworkVariable<bool> _isHeld =
            new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        protected GenericCharacter _owner;
        protected Highlight _highlight;
        protected Rigidbody _rb;

        public ItemStats ItemStats => itemStats;

        /// <summary>True while equipped in a mouth. Not pickable when true.</summary>
        public bool IsHeld => _isHeld.Value;

        // IWorldItem: count is authoritative. Only the server writes it (pickup
        // shrinks the stack); replicate it if clients need to show stack size.
        public int Count
        {
            get => count;
            set => count = Mathf.Max(0, value);
        }

        /// <summary>
        /// Server-only: mark this item as held (equipped) or loose (in world).
        /// Called by the equip flow on spawn, and by drop if it ever reuses an
        /// instance. Held items are skipped by hover and rejected by pickup.
        /// </summary>
        public void SetHeld(bool held)
        {
            if (!IsServer) return;
            _isHeld.Value = held;
        }

        protected virtual void Awake()
        {
            _highlight = GetComponent<Highlight>();
            _rb = GetComponent<Rigidbody>();
        }

        // ---- IInteractable ----

        public void OnInteract(GenericCharacter owner) => _owner = owner;

        public HoverInfoStats GetHoverInfoStats() => hoverInfo;

        // Not hoverable while held (so no highlight on an equipped item) or once
        // claimed by a picker.
        public bool CanHover() => !IsHeld && !_owner;

        public void OnHover() => _highlight.enabled = true;

        public void OnHoverEnd() => _highlight.enabled = false;
    }
}