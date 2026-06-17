using System;

namespace InventorySystem.Core
{
    /// <summary>
    /// Pure value type so it drops into a NetworkList (unmanaged + IEquatable)
    /// Mutate by replacing the whole cell (cells[i] = newCell), never in place.
    /// </summary>
    public readonly struct InventoryCell : IEquatable<InventoryCell>
    {
        public static readonly InventoryCell Empty = new(-1, 0);

        public readonly int ItemId;
        public readonly int Count;

        public InventoryCell(int itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }

        public bool IsEmpty => ItemId < 0 || Count <= 0;

        /// <summary>Same item, different count. Clamps to Empty at or below zero.</summary>
        public InventoryCell WithCount(int count) => count <= 0 ? Empty : new InventoryCell(ItemId, count);

        /// <summary>
        /// Convenience resolve through the active database. Edge use only:
        /// UI and gameplay call this when they need a sprite or prefab.
        /// InventoryOps never touches it, which is what keeps the stacking
        /// math database-free and unit testable.
        /// </summary>
        public ItemStats Definition => ItemRegistry.Resolve(ItemId);

        public bool Equals(InventoryCell other) =>
            ItemId == other.ItemId && Count == other.Count;

        public override bool Equals(object obj) => obj is InventoryCell o && Equals(o);

        public override int GetHashCode() => unchecked((ItemId * 397) ^ Count);

        public static bool operator ==(InventoryCell a, InventoryCell b) => a.Equals(b);
        public static bool operator !=(InventoryCell a, InventoryCell b) => !a.Equals(b);

        public override string ToString() => IsEmpty ? "[empty]" : $"[id:{ItemId} x{Count}]";
    }
}