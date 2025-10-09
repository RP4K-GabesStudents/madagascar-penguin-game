using Scriptable_Objects;
using UnityEngine;

namespace Game.InventorySystem
{
    [CreateAssetMenu(fileName = "ItemStats", menuName = "Scriptable Objects/ItemStats")]
    public class ItemStats : HoverInfoStats
    {
        //[SerializeField] make enum rarity homework
        [SerializeField] private Sprite icon;
        [SerializeField] private int coolDownTime;
        [SerializeField, Min(1)] private int itemLimit;
        [SerializeField] private Item itemPrefab;
        
        
        public Sprite Icon => icon;
        public int CoolDownTime => coolDownTime;
        public int ItemLimit => itemLimit;
        public Item ItemPrefab => itemPrefab;

        private void OnValidate()
        {
            itemLimit = Mathf.Max(1, itemLimit);
        }
        
        
    }
}
