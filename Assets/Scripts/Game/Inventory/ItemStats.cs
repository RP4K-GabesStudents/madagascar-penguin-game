using System;
using Scriptable_Objects;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "ItemStats", menuName = "Scriptable Objects/ItemStats")]
    public class ItemStats : HoverInfoStats
    {
        //[SerializeField] make enum rarity homework
        [SerializeField] private Sprite icon;
        [SerializeField] private int cost;
        [SerializeField, Min(1)] private int itemLimit;
        [SerializeField] private Item itemPrefab;
        
        
        public Sprite Icon => icon;
        public int Cost => cost;
        public int ItemLimit => itemLimit;
        public Item ItemPrefab => itemPrefab;

        private void OnValidate()
        {
            itemLimit = Mathf.Max(1, itemLimit);
        }
        
        
    }
}
