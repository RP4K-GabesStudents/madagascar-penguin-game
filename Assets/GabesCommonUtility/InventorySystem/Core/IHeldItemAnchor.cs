using UnityEngine;

namespace InventorySystem.Core
{
    /// <summary>
    /// Implemented by whatever on the player exposes the transform a held item
    /// should visually track (e.g. a mouth bone). Lets the held item find its
    /// anchor by contract, without depending on a game-specific capability type.
    ///
    /// The anchor may be an animated bone: held items follow it via a
    /// ParentConstraint rather than reparenting, so animation drives the pose
    /// on every client and the item stays a free NetworkObject under the player
    /// root (so it drops cleanly on disconnect).
    /// </summary>
    public interface IHeldItemAnchor
    {
        Transform AnchorTransform { get; }
    }
}