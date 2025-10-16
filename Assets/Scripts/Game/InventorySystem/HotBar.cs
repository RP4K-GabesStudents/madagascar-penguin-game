using System.Collections.Generic;
using UnityEngine;

namespace Game.InventorySystem
{
    public class HotBar : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        
        private Itemslot _selectedItem;
        public int SelectedItemIndex => _curScrollIndex;

        private int _curScrollIndex = 0;

        private Inventory _inventory; // What it reads from.

        [SerializeField] private Itemslot prefab;//fill this out 10/08/25
        private List<Itemslot> itemSlots = new ();

        public void UpdateScrollIndex(int index)
        {
            _selectedItem.MarkUnselected();
            itemSlots[index].MarkSelected();
            _selectedItem = itemSlots[index];
            _curScrollIndex = index;
        }

        public void UpdateScrollSlot(int index)
        {
            if (index == 0) return;
            _curScrollIndex += index;
            if (_curScrollIndex >= itemSlots.Count)
            {
                _curScrollIndex = 0;
            }
            else if (_curScrollIndex < 0)
            {
                _curScrollIndex = itemSlots.Count - 1;
            }

            UpdateScrollIndex(_curScrollIndex);
        }

        private void UpdateSlots()
        {
            if (_inventory.items.Length != itemSlots.Count) RegenerateInventorySlots();
            for (int i = 0; i < itemSlots.Count; i++)
            {
                var container = _inventory.items[i];
                itemSlots[i].SetItem(container.currentItem?.ItemStats, container.CurrentStackSize);
            }
        }

        private void RegenerateInventorySlots()
        {
            for (int i = itemSlots.Count - 1; i >= 0; i--)
            {
                Destroy(itemSlots[i].gameObject);
            }

            for (int i = 0; i < _inventory.items.Length; i++)
            {
                itemSlots.Add(Instantiate(prefab, parent));
            }
        }

        private void Subscribe()
        {
            _inventory.OnInventoryChanged += UpdateSlots;
        }

        private void Unsubscribe()
        {
            if (!_inventory) return;
            _inventory.OnInventoryChanged -= UpdateSlots;
        }

        public void SetTargetInventory(Inventory inventory)
        {
            Unsubscribe();
            _inventory = inventory;
            Subscribe();
            UpdateSlots();
        }
    }
}