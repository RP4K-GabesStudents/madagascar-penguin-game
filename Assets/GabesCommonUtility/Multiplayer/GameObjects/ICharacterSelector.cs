using System;

namespace Multiplayer.GameObjects
{
    /// <summary>
    /// Implemented by whatever drives character selection in your game (e.g. your
    /// SelectionManager). The selection sequence depends on this interface, not on
    /// your game-specific UI, so the multiplayer package stays portable.
    ///
    /// Wire your concrete manager to set CharacterSelector.Active in its Awake/OnEnable
    /// and clear it in OnDestroy.
    /// </summary>
    public interface ICharacterSelector
    {
        /// <summary>Raised when the local player picks a character. The ulong is the
        /// chosen prefab's NetworkObject.PrefabIdHash.</summary>
        event Action<ulong> LocalCharacterChosen;

        /// <summary>
        /// The prefab-id hash of the character the local player is currently looking
        /// at (the highlighted selector). Read by the sequence at window close so a
        /// player who never confirmed still gets the one they were hovering, if free.
        /// 0 means "no meaningful hover".
        /// </summary>
        ulong CurrentHoverPrefabId { get; }

        /// <summary>Called by the sequence when the server rejected a pick (already
        /// taken). Re-open navigation / play a buzzer here.</summary>
        void OnSelectionRejected(ulong prefabId);

        /// <summary>
        /// Driven every frame by the sequence from the server-synced timer.
        /// normalized is time remaining as 1 -> 0. The UI maps this straight onto
        /// the gradient fill. Called on every peer, so all players see the same bar.
        /// </summary>
        void SetTimeRemaining(float normalized);

        /// <summary>
        /// Called once on every peer when the selection window closes (timer hit
        /// zero on the server and the synced done-flag flipped). The UI should lock
        /// input and stop the bar here. Spawning happens in a later sequence step.
        /// </summary>
        void SelectionFinished();
    }

    /// <summary>Static hand-off point so the sequence can find the active selector
    /// without a hard assembly reference to your UI code.</summary>
    public static class CharacterSelector
    {
        public static ICharacterSelector Active { get; set; }
    }

    /// <summary>
    /// Optional server-side name resolver. Your lobby/game layer (which lives in a
    /// higher assembly that can reference this one) registers an implementation, and
    /// the selection sequence reads display names through it by stable playerId.
    /// This keeps the multiplayer package from depending on your Managers assembly
    /// (which would be a circular reference, since that assembly references this one).
    ///
    /// Returns false if no name is known for the id; the caller falls back to a
    /// default. Only meaningful on the host/server, which holds the full roster.
    /// </summary>
    public interface IPlayerNameProvider
    {
        bool TryGetPlayerName(string playerId, out string playerName);
    }

    /// <summary>Static hand-off point for the name provider, mirroring CharacterSelector.</summary>
    public static class PlayerNameDirectory
    {
        public static IPlayerNameProvider Active { get; set; }

        /// <summary>Resolve a name by playerId, or return the given fallback.</summary>
        public static string ResolveOr(string playerId, string fallback)
        {
            if (Active != null && !string.IsNullOrEmpty(playerId)
                && Active.TryGetPlayerName(playerId, out var name)
                && !string.IsNullOrEmpty(name))
                return name;
            return fallback;
        }
    }
}