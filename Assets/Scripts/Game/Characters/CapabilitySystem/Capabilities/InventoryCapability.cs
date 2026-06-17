using Common.Extensions;
using Game.InventorySystem;
using Game.Items.Weapons;
using InventorySystem.Core;
using InventorySystem.Model;
using InventorySystem.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    /// <summary>
    /// Owns the player's inventory DATA, its hotbar UI, the input that drives
    /// drop/attack, AND the live held item in the mouth.
    ///
    /// Hotbar selection input (number keys + scroll) is owned by the HotBar
    /// itself; this capability only reacts to its SelectionChanged event to
    /// re-equip. Attack and Drop remain on GameControls here, since they aren't
    /// hotbar concerns.
    ///
    /// The bag is int ids + counts (InventoryCell), never live objects. When
    /// the selected slot changes, this spawns the selected item's prefab as a
    /// NetworkObject and parents it into the mouth anchor, then routes Attack-
    /// hold to that object's IHeldItem.
    ///
    /// Backend-agnostic: _inventory is an IInventory, so this works identically
    /// for a local Inventory or a NetworkInventory. All spawn/despawn/reparent
    /// is server-authoritative; the owning client drives selection (local) and
    /// forwards Attack as a hold state, since only the server holds the live
    /// item and may call its IHeldItem lifecycle.
    ///
    /// Why spawn-on-equip rather than keeping the picked-up object alive:
    ///  - Stacks. "potion x5" is one cell, not five objects; there is nothing
    ///    single to reparent, so equip spawns exactly one held instance.
    ///  - Netcode reparenting rules. We only reparent on the SERVER via
    ///    NetworkObject.TrySetParent onto the player's already-spawned mouth
    ///    anchor. No client-side SetParent, so the transform never desyncs.
    /// </summary>
    public class InventoryCapability : NetworkBehaviour, IInputSubscriber, IHeldItemAnchor
    {
        [SerializeField] private Transform parent;     // mouth anchor; held items parent here
        [SerializeField] private HotBar hotBarPrefab;  // UI view, instantiated at bind time

        private IInventory _inventory;
        private HotBar _hotBar;
        private GameControls _reference;

        // Server-only held state.
        private NetworkObject _heldInstance;
        private IHeldItem _heldBehaviour;
        private int _heldItemId = -1;
        private bool _useHeld;

        public Transform Parent => parent;

        // IHeldItemAnchor: held items follow this (the mouth bone) via a
        // ParentConstraint on each client.
        public Transform AnchorTransform => parent;

        public int SelectedIndex => _hotBar ? _hotBar.SelectedIndex : 0;

        public InventoryCell CurrentCell =>
            _inventory != null && (uint)SelectedIndex < (uint)_inventory.Capacity
                ? _inventory[SelectedIndex]
                : InventoryCell.Empty;

        /// <summary>
        /// The weapon currently selected, or null if the selected slot isn't a
        /// weapon (or is empty). Resolved from the cell's prefab via the
        /// registry, NOT from the live held instance, so any peer (the owner's
        /// movement/jump/laser capabilities) can read its WeaponStats without
        /// touching the server-spawned object. Stats are asset data, identical
        /// everywhere, so this needs no sync.
        ///
        /// Note this returns the PREFAB's component (good for reading Stats),
        /// not the live in-mouth instance. Read modifiers off .Stats; don't
        /// call instance behaviour on it.
        /// </summary>
        public GenericWeapon EquippedWeapon
        {
            get
            {
                var cell = CurrentCell;
                if (cell.IsEmpty) return null;
                var def = cell.Definition;
                if (def == null || def.ItemPrefab == null) return null;
                return def.ItemPrefab.GetComponent<GenericWeapon>();
            }
        }

        #region Lifecycle

        public override void OnNetworkSpawn()
        {
            enabled = IsOwner; // mirror BaseCapability: only the owner runs input

            // Resolve the inventory on EVERY peer, not just the owner. The
            // server runs ServerDrop/ServerEquip and needs the authoritative
            // instance even on a dedicated server where BindControls (owner-only
            // input setup) never runs.
            if (_inventory == null) _inventory = GetComponentInParent<IInventory>();
            if (_inventory != null) _inventory.OnSlotChanged += OnSlotChanged;

            // Equip whatever is already selected once we're live.
            if (IsOwner) RequestEquip();
        }

        public override void OnNetworkDespawn()
        {
            if (_inventory != null) _inventory.OnSlotChanged -= OnSlotChanged;
            if (IsServer) DespawnHeld();
        }

        #endregion

        #region Controls

        public void BindControls(GameControls controls)
        {
            // Normally resolved in OnNetworkSpawn; resolve here too in case
            // binding runs first.
            if (_inventory == null) _inventory = GetComponentInParent<IInventory>();
            if (_inventory == null)
            {
                Debug.LogError($"[InventoryCapability] No IInventory found in parents of {name}.", gameObject);
                return;
            }

            if (!_hotBar)
            {
                _hotBar = Instantiate(hotBarPrefab);
                _hotBar.Bind(_inventory);
                _hotBar.SelectionChanged += OnHotBarSelectionChanged;
                _hotBar.Select(0);
            }

            _inventory.OnSlotChanged -= OnSlotChanged;
            _inventory.OnSlotChanged += OnSlotChanged;

            Unsubscribe();
            _reference = controls;
            Subscribe();
        }

        private void Subscribe()
        {
            if (_reference == null) return;

            _reference.Player.Attack.performed += OnAttack;
            _reference.Player.Attack.canceled  += OnAttack;
            _reference.Player.Drop.performed += DropCurrentItem;
        }

        private void Unsubscribe()
        {
            if (_reference == null) return;

            _reference.Player.Attack.performed -= OnAttack;
            _reference.Player.Attack.canceled  -= OnAttack;
            _reference.Player.Drop.performed -= DropCurrentItem;
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        #endregion

        #region Selection

        // The HotBar owns scroll + number-key input and raises this when the
        // selected index actually changes. Re-equip to match.
        private void OnHotBarSelectionChanged(int index) => RequestEquip();

        // Replication / local mutation landed. If the data under the held slot
        // changed (e.g. the last potion was consumed), re-evaluate what's held.
        private void OnSlotChanged(int index)
        {
            if (index == SelectedIndex) RequestEquip();
        }

        #endregion

        #region Equip flow

        private void OnAttack(InputAction.CallbackContext obj)
        {
            // Hold context: down -> start, up -> stop. The live item is on the
            // server, so forward the intent rather than calling locally.
            bool pressed = obj.ReadValueAsButton();
            SetUse_Rpc(pressed);
        }

        [Rpc(SendTo.Server)]
        private void SetUse_Rpc(bool pressed)
        {
            _useHeld = pressed;
            if (_heldBehaviour == null) return;

            if (pressed) _heldBehaviour.OnStartUse();
            else _heldBehaviour.OnStopUse();
        }

        private void RequestEquip()
        {
            // Resolve target id from the selected cell and ask the server to
            // make the live object match. Send the id, not the slot, so the
            // server never re-reads client-side selection.
            InventoryCell cell = CurrentCell;
            int itemId = cell.IsEmpty ? -1 : cell.ItemId;

            if (IsServer) ServerEquip(itemId);
            else Equip_Rpc(itemId);
        }

        [Rpc(SendTo.Server)]
        private void Equip_Rpc(int itemId) => ServerEquip(itemId);

        private void ServerEquip(int itemId)
        {
            // Already holding the right thing: no-op (prevents respawn churn
            // when scrolling onto the same type or on count changes).
            if (itemId == _heldItemId && _heldInstance != null) return;

            DespawnHeld();
            if (itemId < 0) return;

            ItemStats def = ItemRegistry.Resolve(itemId);
            if (def == null || def.ItemPrefab == null)
            {
                Debug.LogError($"[InventoryCapability] Cannot equip id {itemId}: no prefab.", gameObject);
                return;
            }

            Transform mouth = parent;

            GameObject go = Instantiate(def.ItemPrefab, mouth.position, mouth.rotation);
            _heldInstance = go.GetComponent<NetworkObject>();
            if (_heldInstance == null)
            {
                Debug.LogError($"[InventoryCapability] {def.name} prefab has no NetworkObject.", go);
                Destroy(go);
                return;
            }

            _heldInstance.Spawn(); // server-spawned, server-owned

            // Mark it held so it isn't a pickup target while equipped (the held
            // item is the same prefab as the world pickup; without this it could
            // be re-absorbed and duplicated, or stolen by another player).
            if (go.TryGetComponent(out WorldItem worldItem)) worldItem.SetHeld(true);

            // NGO only allows parenting under another spawned NetworkObject, so
            // we parent to the player ROOT (which is one), NOT the mouth anchor
            // (a plain child Transform). The item then visually tracks the
            // animated mouth bone via a HeldItemAnchorFollower / ParentConstraint
            // on EACH client, so we don't set any local offset here.
            var playerRoot = GetRootNetworkObject();
            if (playerRoot == null || !_heldInstance.TrySetParent(playerRoot, false))
                Debug.LogWarning($"[InventoryCapability] TrySetParent failed for {def.name}.", go);

            _heldBehaviour = go.GetComponentInChildren<IHeldItem>();
            _heldBehaviour?.OnEquip(ResolveOwner());
            _heldItemId = itemId;

            if (_useHeld) _heldBehaviour?.OnStartUse(); // carry hold across a swap
        }

        private void DespawnHeld()
        {
            if (_heldInstance == null)
            {
                _heldItemId = -1;
                _heldBehaviour = null;
                return;
            }

            _heldBehaviour?.OnStopUse();
            _heldBehaviour?.OnUnequip();

            if (_heldInstance.IsSpawned) _heldInstance.Despawn();
            else Destroy(_heldInstance.gameObject);

            _heldInstance = null;
            _heldBehaviour = null;
            _heldItemId = -1;
        }

        // The owning character, for IHeldItem.OnEquip. Resolved off the root so
        // this class doesn't need a BaseCapability base.
        private Game.Characters.World.GenericCharacter ResolveOwner() =>
            transform.root.GetComponent<Game.Characters.World.GenericCharacter>();

        // The player's root NetworkObject, used as the parent for held items
        // (NGO requires a NetworkObject parent). Prefer the transform root;
        // fall back to this behaviour's own NetworkObject.
        private NetworkObject GetRootNetworkObject()
        {
            var root = transform.root.GetComponent<NetworkObject>();
            return root != null ? root : NetworkObject;
        }

        #endregion

        #region Drop

        [Header("Drop")]
        [Tooltip("Impulse applied to a dropped world item, in mouth-local space (z = forward).")]
        [SerializeField] private Vector3 dropImpulse = new Vector3(0f, 1.5f, 3f);

        private void DropCurrentItem(InputAction.CallbackContext _) => RequestDrop(SelectedIndex);

        /// <summary>
        /// Drop the whole stack in the currently selected slot, over the net.
        /// The owner forwards the slot to the server, which removes the data and
        /// spawns the world prefab. Sends the slot, not the id: the server reads
        /// its own authoritative cell rather than trusting client-sent contents.
        /// Does nothing if the slot is empty.
        /// </summary>
        private void RequestDrop(int slot)
        {
            if (IsServer) ServerDrop(slot);
            else Drop_Rpc(slot);
        }

        [Rpc(SendTo.Server)]
        private void Drop_Rpc(int slot) => ServerDrop(slot);

        private void ServerDrop(int slot)
        {
            if (_inventory == null) return;
            if ((uint)slot >= (uint)_inventory.Capacity) return;

            // Capture id + count BEFORE removing: the cell is a value type, and
            // after removal the slot reads Empty, so we'd lose what to spawn.
            InventoryCell cell = _inventory[slot];
            if (cell.IsEmpty) return; // nothing held: do nothing

            // Server is authoritative here (Drop_Rpc routed us here), so the
            // remove applies immediately and OnSlotChanged fires for this slot,
            // clearing the held visual via RequestEquip on its own.
            if (!_inventory.TryRemoveAt(slot, cell.Count)) return;

            SpawnWorldItem(cell);
        }

        private void SpawnWorldItem(InventoryCell dropped)
        {
            ItemStats def = dropped.Definition;
            if (def == null || def.ItemPrefab == null)
            {
                Debug.LogError($"[InventoryCapability] Cannot drop {dropped}: no prefab.", gameObject);
                return;
            }

            Transform mouth = parent;
            GameObject go = Instantiate(def.ItemPrefab, mouth.position, mouth.rotation);

            var netObj = go.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError($"[InventoryCapability] {def.name} prefab has no NetworkObject.", go);
                Destroy(go);
                return;
            }

            // Stack size travels with the world object so it can be picked back
            // up as the same count.
            var worldItem = go.GetComponentInChildren<IWorldItem>();
            if (worldItem != null) worldItem.Count = dropped.Count;

            netObj.Spawn(); // server-spawned, world-space (not parented to the player)

            // Push it out from the mouth so it doesn't drop straight onto us.
            if (go.TryGetComponent(out Rigidbody rb))
                rb.AddForce(mouth.TransformDirection(dropImpulse), ForceMode.Impulse);
        }

        #endregion
    }
}