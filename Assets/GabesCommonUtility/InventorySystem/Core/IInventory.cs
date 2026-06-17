using System;

namespace InventorySystem.Core
{
    /// <summary>
    /// The contract UI and gameplay depend on, so neither ever knows whether
    /// the backing store is a plain array or a replicated NetworkList.
    ///
    /// Mutation semantics differ by backend, and callers must respect this:
    ///  - Inventory (local): mutations apply immediately, return values are truth.
    ///  - NetworkInventory on the SERVER: same, immediate and authoritative.
    ///  - NetworkInventory on a CLIENT: mutations are requests sent to the
    ///    server. The return value only means "request submitted". Real state
    ///    arrives via replication and fires OnSlotChanged when it lands.
    /// UI should therefore render from OnSlotChanged, never from return values.
    /// </summary>
    public interface IInventory
    {
        int Capacity { get; }

        InventoryCell this[int index] { get; }

        /// <summary>Fired with the slot index that changed. UI binds to this.</summary>
        event Action<int> OnSlotChanged;

        /// <summary>Two-pass stacking add. remainder = what did not fit (only meaningful where authoritative).</summary>
        bool TryAdd(int itemId, int count, out int remainder);

        /// <summary>Remove count from one slot. Fails if the slot cannot cover it.</summary>
        bool TryRemoveAt(int index, int count);

        /// <summary>Move/merge/swap between two slots. The drag-and-drop op.</summary>
        bool TryMove(int from, int to);
    }
}