using System;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
using UnityEngine.Serialization;

namespace Inventory
{
    public class AnInventory : MonoBehaviour
    {
        private enum ItemRarityColour
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary,
            Mythic,
            Penguin
        }
        [SerializeField] private ItemRarityColour itemRarityColour = ItemRarityColour.Common;
        [SerializeField] private Itemslot itemslot;
        

        private void Awake()
        {
            
        }
        
        
    }
}
