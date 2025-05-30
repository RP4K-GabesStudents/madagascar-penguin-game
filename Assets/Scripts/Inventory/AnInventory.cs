
using UnityEngine;
using Utilities;


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
        [SerializeField] private Itemslot[] itemSlots;
        private Itemslot selectedItem;
        public Itemslot SelectedItem => selectedItem;

        private void Start()
        {
            foreach (var jeff in itemSlots)
            {
                jeff.RemoveItem();
            }

            selectedItem = itemSlots[0];
            selectedItem.MarkSelected();
        }

        [ContextMenu("find")]
        private void FindItemSlots()
        {
            itemSlots = GetComponentsInChildren<Itemslot>();
        }

        //puts an item in the item slot
        public bool HeyIPickedSomethingUp(ItemStats item)
        {
            Itemslot apple = null;
            foreach (var jeff in itemSlots)
            {
                if (jeff.CanAcceptItem(item))
                {
                    jeff.SetItem(item);
                    apple = jeff;
                    break;
                }
            }

            if (apple == null) return false;
            if (Settings.GamePlaySettings.autoEquip)
            {
                selectedItem.MarkUnselected();
                apple.MarkSelected();
                selectedItem = apple;
            }
            return true;
        }

    }
}
