using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities.Utilities.General;

namespace Game.InventorySystem
{
    public class Inventory : MonoBehaviour
    {
        public ItemContainer[] items = new ItemContainer[10];
        public event Action OnInventoryChanged;
        
        public bool TryDropItem(int slot, out Item item)
        {
            
            if (!items.IsValidIndex(slot))
            {
                Debug.LogError("this is a very serious problem fix immediately");
                item = null;
                return false;    
            }
            
            var itemSlot = items[slot];
            item = itemSlot.CurrentItem;
            if (item == null) return false;

            itemSlot.TakeItem(1);
            //Austin fill this out... im trying man
            OnInventoryChanged?.Invoke();
            Debug.LogError("Austin do dropping >:). im trying");
            
            return true;
        }

        public int TryPickup(Item item, int stackSize = 1)
        {
            //iterate through the inventory, choose left most spot
            for (var index = 0; index < items.Length; index++)
            {
                var itemContainer = items[index];
                if (itemContainer.CanAcceptItem(item, stackSize)) // Try to accept the stack size...
                {
                    itemContainer.SetItem(item, stackSize);
                    OnInventoryChanged?.Invoke();
                    return index;
                }
            }
            return -1;
        }
    }
}
