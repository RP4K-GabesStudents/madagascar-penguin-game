using System;
using InventorySystem.Core;
using InventorySystem.Model;
using Unity.Netcode;
using UnityEngine;

namespace InventorySystem.Netcode
{
    /// <summary>
    /// Networked inventory: a server-authoritative NetworkList of cells.
    ///
    /// Flow: client calls TryAdd/TryRemoveAt/TryMove -> ServerRpc -> server
    /// validates and runs the same InventoryOps math -> writes changed entries
    /// back to the NetworkList -> replication fires OnListChanged on everyone
    /// -> re-raised as OnSlotChanged, the same event the local backend fires.
    /// The UI cannot tell the backends apart, which is the whole point.
    ///
    /// Storage detail: NGO requires NetworkList elements to implement
    /// INetworkSerializeByMemcpy, an NGO interface that must not leak into the
    /// core assembly. NetCell is the netcode-side twin of InventoryCell that
    /// carries the marker; implicit conversions at the boundary keep the rest
    /// of this class, and all of core, speaking InventoryCell.
    /// </summary>
    public class NetworkInventory : NetworkBehaviour, IInventory
    {
        /// <summary>
        /// Wire twin of InventoryCell. Same layout plus NGO's memcpy marker.
        /// Private on purpose: nothing outside this class should ever see it.
        /// </summary>
        private struct NetCell : INetworkSerializeByMemcpy, IEquatable<NetCell>
        {
            public int ItemId;
            public int Count;

            public static implicit operator InventoryCell(NetCell c) => new(c.ItemId, c.Count);
            public static implicit operator NetCell(InventoryCell c) => new() { ItemId = c.ItemId, Count = c.Count };

            public bool Equals(NetCell other) => ItemId == other.ItemId && Count == other.Count;
            public override bool Equals(object obj) => obj is NetCell o && Equals(o);
            public override int GetHashCode() => unchecked((ItemId * 397) ^ Count);
        }

        [SerializeField, Min(1)] private int capacity = 10;

        // NGO rule: construct NetworkLists in the field initializer, never in
        // OnNetworkSpawn, or early replication deltas can arrive before the
        // list exists.
        private readonly NetworkList<NetCell> _cells = new();

        private InventoryCell[] _scratch; // staging buffer for InventoryOps

        public int Capacity => _cells.Count;
        public InventoryCell this[int index] => _cells[index];

        public event Action<int> OnSlotChanged;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                for (int i = 0; i < capacity; i++)
                    _cells.Add(InventoryCell.Empty);

            _cells.OnListChanged += HandleListChanged;
        }

        public override void OnNetworkDespawn()
        {
            _cells.OnListChanged -= HandleListChanged;
        }

        private void HandleListChanged(NetworkListEvent<NetCell> e)
        {
            OnSlotChanged?.Invoke(e.Index);
        }

        // ---- IInventory: each op runs locally on the server, or becomes a
        // ---- request when called on a client. See IInventory doc for semantics.

        public bool TryAdd(int itemId, int count, out int remainder)
        {
            remainder = count;
            if (IsServer) return ServerAdd(itemId, count, out remainder);

            RequestAdd_ServerRpc(itemId, count);
            remainder = 0;
            return true; // request submitted; truth arrives via replication
        }

        public bool TryRemoveAt(int index, int count)
        {
            if (IsServer) return ServerRemoveAt(index, count);
            RequestRemoveAt_ServerRpc(index, count);
            return true;
        }

        public bool TryMove(int from, int to)
        {
            if (IsServer) return ServerMove(from, to);
            RequestMove_ServerRpc(from, to);
            return true;
        }

        // ---- Server-side implementations: stage, run ops, write back. ----

        private bool ServerAdd(int itemId, int count, out int remainder)
        {
            remainder = count;
            var def = ItemRegistry.Resolve(itemId);
            if (!def) return false;

            var span = Stage();
            bool changed = InventoryOps.TryAdd(span, itemId, count, def.StackSize, out remainder);
            if (changed) Commit();
            return changed;
        }

        private bool ServerRemoveAt(int index, int count)
        {
            if ((uint)index >= (uint)_cells.Count) return false;

            var span = Stage();
            bool changed = InventoryOps.TryRemoveAt(span, index, count);
            if (changed) Commit();
            return changed;
        }

        private bool ServerMove(int from, int to)
        {
            if ((uint)from >= (uint)_cells.Count || (uint)to >= (uint)_cells.Count) return false;

            var def = ItemRegistry.Resolve(_cells[from].ItemId);
            int maxStack = def ? def.StackSize : 1;

            var span = Stage();
            bool changed = InventoryOps.TryMove(span, from, to, maxStack);
            if (changed) Commit();
            return changed;
        }

        /// <summary>Copy the NetworkList into the scratch buffer and hand ops a span over it.</summary>
        private Span<InventoryCell> Stage()
        {
            if (_scratch == null || _scratch.Length != _cells.Count)
                _scratch = new InventoryCell[_cells.Count];
            for (int i = 0; i < _cells.Count; i++) _scratch[i] = _cells[i];
            return _scratch;
        }

        /// <summary>
        /// Write only changed entries back, so replication sends deltas for
        /// exactly the slots that moved and OnListChanged fires per slot.
        /// </summary>
        private void Commit()
        {
            for (int i = 0; i < _cells.Count; i++)
                if ((InventoryCell)_cells[i] != _scratch[i])
                    _cells[i] = _scratch[i];
        }

        // ---- Client -> server requests. Server validates; the sender finds
        // ---- out by watching the list change (or not).

        [ServerRpc(RequireOwnership = false)]
        private void RequestAdd_ServerRpc(int itemId, int count)
        {
            // Validate: only sane amounts. Anything beyond this (does this
            // client even own this inventory?) belongs in your session rules.
            if (count <= 0) return;
            ServerAdd(itemId, count, out _);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestRemoveAt_ServerRpc(int index, int count)
        {
            if (count <= 0) return;
            ServerRemoveAt(index, count);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestMove_ServerRpc(int from, int to)
        {
            ServerMove(from, to);
        }
    }
}