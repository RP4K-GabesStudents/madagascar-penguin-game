using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace InventorySystem.Core
{
    [CreateAssetMenu(fileName = "ItemStats", menuName = "Items/ItemStats")]
    public class ItemStats : ScriptableObject
    {
        // Stable, runtime-resolvable id derived from the asset GUID (see OnValidate).
        // Always >= 0; InventoryCell reserves id < 0 for 'empty'. Never hand-type it.
        [field: FormerlySerializedAs("<id>k__BackingField")] [field: SerializeField, ReadOnly] public int ID {get; private set;} = -1;
        [field: FormerlySerializedAs("<rarity>k__BackingField")] [field: SerializeField] public EItemRarity Rarity {get; private set;}
        [field: FormerlySerializedAs("<icon>k__BackingField")] [field: SerializeField] public Sprite Icon {get; private set;}
        [field: FormerlySerializedAs("<stackSize>k__BackingField")] [field: FormerlySerializedAs("itemLimit"), SerializeField, Min(1)] public int StackSize {get; private set;}
        [field: FormerlySerializedAs("<itemPrefab>k__BackingField")] [field: SerializeField] public GameObject ItemPrefab {get; private set;}
        
#if UNITY_EDITOR  
        private void OnValidate()
        {
            StackSize = Mathf.Max(1, StackSize); // << Safety Force

            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(path))
                ID = StableId(UnityEditor.AssetDatabase.AssetPathToGUID(path));

            var def = ItemPrefab.GetComponentInChildren<IWorldItem>();
            if (def == null)
            {
                Debug.LogError("ItemPrefab must have an IItemDefinition component", this);
            }
        }
        // FNV-1a over the asset GUID. Sign bit cleared so id is always >= 0,
        // because InventoryCell treats id < 0 as its 'empty' sentinel.
        private static int StableId(string guid)
        {
            uint hash = 2166136261;
            foreach (char c in guid)
            {
                hash ^= c;
                hash *= 16777619;
            }
            return (int)(hash & 0x7FFFFFFF);
        }
#endif
    }
}