using Inventory;
using UnityEngine;

namespace Game.Inventory
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
        [SerializeField] protected Itemslot[] itemSlots;
        [ContextMenu("find")]
        private void FindItemSlots()
        {
            itemSlots = GetComponentsInChildren<Itemslot>();
        }

        //puts an item in the item slot
        public bool HeyIPickedSomethingUp(ItemStats item)
        {
            int apple = -1;
            for (var i = 0; i < itemSlots.Length; i++)
            {
                var jeff = itemSlots[i];
                if (jeff.CanAcceptItem(item))
                {
                    jeff.SetItem(item);
                    apple = i;
                    break;
                }
            }

            if (apple == -1) return false;
            OnSlotUpdated(apple);
            return true;
        }
        
        protected virtual void OnSlotUpdated(int index)
        {
            
        }
    }
}
