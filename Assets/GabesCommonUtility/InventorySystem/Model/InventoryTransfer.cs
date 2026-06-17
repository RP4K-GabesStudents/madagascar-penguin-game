using System;
using InventorySystem.Core;

namespace InventorySystem.Model
{
    /// <summary>
    /// The two transfer operations between world and bag, and the only
    /// package code that touches IItem. Sits above InventoryOps (pure cell
    /// math) and below the game (object lifetime, spawning).
    ///
    /// Pickup: world object -> cells. Drop: cells -> data the game spawns from.
    /// Both are authoritative-context only: on a networked client the
    /// underlying mutations are fire-and-forget requests, so run these on the
    /// server (your interact / drop RPCs), never client-side.
    /// </summary>
    public static class InventoryTransfer
    {
        /// <summary>
        /// Move as much of the world stack into the inventory as fits.
        /// Returns true if anything transferred. On return item.Count is
        /// already shrunk; 0 means the caller should remove the world object.
        /// </summary>
        public static bool TryPickup(this IInventory inventory, IWorldItem worldItem)
        {
            if (worldItem?.ItemStats == null || worldItem.Count <= 0) return false;

            if (!inventory.TryAdd(worldItem.ItemStats.ID, worldItem.Count, out int remainder))
                return false;

            worldItem.Count = remainder;
            return true;
        }

        /// <summary>
        /// Extract up to 'count' from a slot as data for the game to spawn
        /// from. Clamps to what the slot holds, so "drop the whole stack" is
        /// TryDropAt(slot, int.MaxValue, out var dropped).
        ///
        /// The package ends at the returned cell. The game finishes the job:
        /// resolve dropped.Definition, instantiate its ItemPrefab, set the
        /// spawned IItem.Count to dropped.Count.
        /// </summary>
        public static bool TryDropAt(this IInventory inventory, int index, int count, out InventoryCell dropped)
        {
            dropped = InventoryCell.Empty;
            if ((uint)index >= (uint)inventory.Capacity || count <= 0) return false;

            var cell = inventory[index];
            if (cell.IsEmpty) return false;

            int take = Math.Min(count, cell.Count);
            if (!inventory.TryRemoveAt(index, take)) return false;

            dropped = new InventoryCell(cell.ItemId, take);
            return true;
        }
    }
}