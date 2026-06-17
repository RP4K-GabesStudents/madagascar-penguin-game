using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Core
{
    /// <summary>
    /// The Item Registry is a static map of all items in the game.
    /// For NGO and saves work properly, we must store an ID instead of an object
    /// </summary>
    public static class ItemRegistry
    {
        private static Dictionary<int, ItemStats> _byId;

        public static bool Ready => _byId != null;

        // Re-runs on every entry to play mode, including with domain reload
        // disabled, so the map can never go stale between sessions.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoBuild()
        {
            // Every ItemStats under a Resources/Items folder. Drop an asset in,
            // it's registered. Nothing to wire up, nothing to keep in sync.
            BuildFrom(Resources.LoadAll<ItemStats>("Items"));
        }

        /// <summary>
        /// Explicit build. Edit-mode tests call this with a fake set; a custom
        /// content pipeline (Addressables, a baked list) can call it too.
        /// Also the single place a 32-bit id collision would surface, loudly.
        /// </summary>
        public static void BuildFrom(IReadOnlyList<ItemStats> defs)
        {
            _byId = new Dictionary<int, ItemStats>(defs.Count);

            for (int i = 0; i < defs.Count; i++)
            {
                var def = defs[i];
                if (!def) continue;

                if (def.ID < 0)
                {
                    Debug.LogError($"ItemRegistry: '{def.name}' has no id.", def);
                    continue;
                }

                if (_byId.TryGetValue(def.ID, out var other))
                {
                    Debug.LogError(
                        $"ItemRegistry: id collision {def.ID} between " +
                        $"'{other.name}' and '{def.name}'.", def);
                    continue;
                }

                _byId.Add(def.ID, def);
            }
        }

        public static ItemStats Resolve(int id) =>
            _byId != null && _byId.TryGetValue(id, out var def) ? def : null;
    }
}