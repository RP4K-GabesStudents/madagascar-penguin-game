using System;
using UnityEngine;
using Utilities;

namespace Inventory
{
    public class HotBar : AnInventory
    {
        private Itemslot _selectedItem;
        public Itemslot SelectedItem => _selectedItem;
        private int _curScrollIndex = 0;
        private void Start()
        {
            foreach (var jeff in itemSlots)
            {
                jeff.RemoveItem();
            }
            _selectedItem = itemSlots[_curScrollIndex];
            _selectedItem.MarkSelected();
        }

        protected override void OnSlotUpdated(int index)
        {
            if (Settings.GamePlaySettings.autoEquip)
            {
                UpdateScrollIndex(index);
            }
        }

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
            var indexChange = index / (int)MathF.Abs(index);
            _curScrollIndex += indexChange;
            if (_curScrollIndex >= itemSlots.Length)
            {
                _curScrollIndex = 0;
            }
            else if (_curScrollIndex < 0)
            {
                _curScrollIndex = itemSlots.Length - 1;   
            }
            Debug.Log(indexChange);
            UpdateScrollIndex(_curScrollIndex);
        }
    }
}