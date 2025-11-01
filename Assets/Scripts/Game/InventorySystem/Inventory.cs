using System;
using UnityEngine;

namespace Game.InventorySystem
{
    public class Inventory : MonoBehaviour
    {
        public ItemContainer[] items = new ItemContainer[10];
        public event Action OnInventoryChanged;
        
        public void DropItem(int slot)
        {
            var item = items[slot];
            if (item == null) return;
            
            
            
            item.SetItem(null, 0);
            //Austin fill this out... im trying man
            OnInventoryChanged?.Invoke();
            Debug.LogError("Austin do dropping >:). im trying");
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
