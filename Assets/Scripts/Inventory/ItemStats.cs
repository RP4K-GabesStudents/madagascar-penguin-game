using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "ItemStats", menuName = "Scriptable Objects/ItemStats")]
    public class ItemStats : ScriptableObject
    {
        //[SerializeField] make enum rarity homework
        [SerializeField] private Sprite icon;
        [SerializeField] private int cost;
        [SerializeField] private int itemLimit;
        [SerializeField] private Item itemPrefab;
        
        
        public Sprite Icon => icon;
        public int Cost => cost;
        public int ItemLimit => itemLimit;
        public Item ItemPrefab => itemPrefab;
    }
}
