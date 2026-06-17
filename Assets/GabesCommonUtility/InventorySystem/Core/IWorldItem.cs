namespace InventorySystem.Core
{
    /// <summary>
    /// The package's entire knowledge of an item existing in the world:
    /// what it is and how many. Nothing about interaction, hovering,
    /// physics, or networking; those belong to the implementing game object
    /// on the other side of this boundary.
    ///
    /// Count is writable because a partial pickup shrinks the world stack
    /// in place. Only authoritative code writes it (the server, in a
    /// networked implementation).
    /// </summary>
    public interface IWorldItem
    {
        ItemStats ItemStats { get; }
        int Count { get; set; }
    }
}