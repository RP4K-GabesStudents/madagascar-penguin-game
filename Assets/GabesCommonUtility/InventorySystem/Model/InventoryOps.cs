using System;
using InventorySystem.Core;

namespace InventorySystem.Model
{
    /// <summary>
    /// All stacking math, as pure functions over a span of cells.
    /// No Unity types, no netcode, no database. The caller resolves the
    /// per-item stack limit and passes it in as maxStack, which is the one
    /// thing that keeps this fully testable: a test calls TryAdd with plain
    /// ints and zero Unity objects.
    ///
    /// Both backends call these:
    ///   LocalInventory   -> InventoryOps.TryAdd(cells.AsSpan(), ...)
    ///   NetworkInventory -> copy NetworkList into a scratch buffer, run ops,
    ///                       write changed entries back so replication fires.
    /// </summary>
    public static class InventoryOps
    {
        /// <summary>
        /// Two-pass add: top up existing matching stacks first, then fill empty
        /// slots. 'remainder' is whatever did not fit. Returns true if anything
        /// changed. Atomic-vs-partial is the caller's policy: check remainder
        /// (or call RoomFor first) if you want all-or-nothing.
        /// </summary>
        public static bool TryAdd(Span<InventoryCell> cells, int itemId, int count, int maxStack, out int remainder)
        {
            remainder = count;
            if (itemId < 0 || count <= 0 || maxStack <= 0) return false;

            int start = remainder;

            // Pass 1: merge into existing stacks of the same item.
            for (int i = 0; i < cells.Length && remainder > 0; i++)
            {
                var cell = cells[i];
                if (cell.IsEmpty || cell.ItemId != itemId) continue;

                int space = maxStack - cell.Count;
                if (space <= 0) continue;

                int add = Math.Min(space, remainder);
                cells[i] = new InventoryCell(itemId, cell.Count + add);
                remainder -= add;
            }

            // Pass 2: fill empty slots, one stack at a time.
            for (int i = 0; i < cells.Length && remainder > 0; i++)
            {
                if (!cells[i].IsEmpty) continue;

                int add = Math.Min(maxStack, remainder);
                cells[i] = new InventoryCell(itemId, add);
                remainder -= add;
            }

            return remainder < start;
        }

        /// <summary>How many of itemId could fit right now, without mutating anything.</summary>
        public static int RoomFor(ReadOnlySpan<InventoryCell> cells, int itemId, int maxStack)
        {
            if (itemId < 0 || maxStack <= 0) return 0;

            int room = 0;
            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                if (cell.IsEmpty) room += maxStack;
                else if (cell.ItemId == itemId) room += Math.Max(0, maxStack - cell.Count);
            }
            return room;
        }

        /// <summary>Total count of a given item across all slots.</summary>
        public static int CountOf(ReadOnlySpan<InventoryCell> cells, int itemId)
        {
            if (itemId < 0) return 0;

            int total = 0;
            for (int i = 0; i < cells.Length; i++)
                if (!cells[i].IsEmpty && cells[i].ItemId == itemId) total += cells[i].Count;
            return total;
        }

        /// <summary>Remove up to 'count' from one slot. Fails if the slot can't cover it.</summary>
        public static bool TryRemoveAt(Span<InventoryCell> cells, int index, int count)
        {
            if ((uint)index >= (uint)cells.Length) return false;

            var cell = cells[index];
            if (cell.IsEmpty || count <= 0 || cell.Count < count) return false;

            cells[index] = cell.WithCount(cell.Count - count);
            return true;
        }

        /// <summary>
        /// Move from one slot to another, the operation a UI drag triggers.
        /// Empty target: straight move. Same item: merge what fits, remainder
        /// stays in source. Different items: swap. Same item with a full target
        /// returns false, since nothing useful happens.
        /// maxStack is the moved item's limit (source and target share it on a merge).
        /// </summary>
        public static bool TryMove(Span<InventoryCell> cells, int from, int to, int maxStack)
        {
            if ((uint)from >= (uint)cells.Length || (uint)to >= (uint)cells.Length || from == to)
                return false;

            var src = cells[from];
            if (src.IsEmpty) return false;
            var dst = cells[to];

            if (dst.IsEmpty)
            {
                cells[to] = src;
                cells[from] = InventoryCell.Empty;
                return true;
            }

            if (dst.ItemId == src.ItemId)
            {
                int space = maxStack - dst.Count;
                if (space <= 0) return false;

                int move = Math.Min(space, src.Count);
                cells[to] = new InventoryCell(dst.ItemId, dst.Count + move);
                cells[from] = src.WithCount(src.Count - move);
                return true;
            }

            // Different items: swap.
            cells[to] = src;
            cells[from] = dst;
            return true;
        }
    }
}