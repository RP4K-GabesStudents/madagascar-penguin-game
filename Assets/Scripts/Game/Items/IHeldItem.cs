using Game.Characters.World;

    /// <summary>
    /// The held/equipped half of an item. Lives on the same prefab as the
    /// item's IWorldItem (ground) component, but only matters once the item
    /// is parented into the player's mouth by EquipCapability.
    ///
    /// Lifecycle, all driven by EquipCapability on the SERVER:
    ///   OnEquip    -> a fresh instance was spawned + parented to the mouth.
    ///   OnStartUse -> Attack pressed while this is the held item.
    ///   OnStopUse  -> Attack released (or item is about to be unequipped).
    ///   OnUnequip  -> about to be despawned; release anything you grabbed.
    ///
    /// Note on networking: the implementing component is a NetworkBehaviour,
    /// so it can fire its own RPCs from inside these calls. EquipCapability
    /// invokes the lifecycle on the server; if you need clients to react
    /// (VFX, animation), fan out with a ClientRpc from your implementation.
    /// </summary>
    public interface IHeldItem
    {
        void OnEquip(GenericCharacter owner);
        void OnStartUse();
        void OnStopUse();
        void OnUnequip();
    }

