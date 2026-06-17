using System;
using InventorySystem.Core;
using UnityEngine;

namespace InventorySystem.Model
{
    /// <summary>
    /// Local, non-networked inventory: a plain InventoryCell[] with all math
    /// delegated to InventoryOps. The single-player / offline backend.
    /// </summary>
    public class Inventory : MonoBehaviour, IInventory
    {
        [SerializeField, Min(1)] private int capacity = 10;

        private InventoryCell[] _cells;
        private InventoryCell[] _before; // snapshot for per-slot change events

        public int Capacity => capacity;
        public InventoryCell this[int index] => _cells[index];

        public event Action<int> OnSlotChanged;

        private void Awake()
        {
            _cells = new InventoryCell[capacity];
            _before = new InventoryCell[capacity];
            for (int i = 0; i < capacity; i++) _cells[i] = InventoryCell.Empty;
        }

        public bool TryAdd(int itemId, int count, out int remainder)
        {
            remainder = count;
            var def = ItemRegistry.Resolve(itemId);
            if (!def) return false;

            Snapshot();
            bool changed = InventoryOps.TryAdd(_cells, itemId, count, def.StackSize, out remainder);
            if (changed) FireDiff();
            return changed;
        }

        public bool TryRemoveAt(int index, int count)
        {
            if ((uint)index >= (uint)capacity) return false;

            Snapshot();
            bool changed = InventoryOps.TryRemoveAt(_cells, index, count);
            if (changed) FireDiff();
            return changed;
        }

        public bool TryMove(int from, int to)
        {
            if ((uint)from >= (uint)capacity || (uint)to >= (uint)capacity) return false;

            var def = _cells[from].Definition;       // moved item's stack limit
            int maxStack = def ? def.StackSize : 1;

            Snapshot();
            bool changed = InventoryOps.TryMove(_cells, from, to, maxStack);
            if (changed) FireDiff();
            return changed;
        }

        private void Snapshot() => Array.Copy(_cells, _before, capacity);

        private void FireDiff()
        {
            for (int i = 0; i < capacity; i++)
                if (_cells[i] != _before[i])
                    OnSlotChanged?.Invoke(i);
        }
    }
}