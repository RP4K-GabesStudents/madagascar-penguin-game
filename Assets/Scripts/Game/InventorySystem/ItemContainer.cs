using System;
using UnityEngine;

namespace Game.InventorySystem
{
    [Serializable]
    public class ItemContainer
    {
        private int _currentStackSize;
        private int _stackSize;
        [SerializeField] private Item item;
        public int CurrentStackSize => _currentStackSize;

        public void ReInit()
        {
            if (!item) return;
            _currentStackSize = 1;
            _stackSize = item.ItemStats.ItemLimit;
        }

        public bool CanAcceptItem(Item newItem, int incomingStackSize = 1) => !item ||
                                                                              (newItem.ItemStats == item.ItemStats &&
                                                                               _stackSize - _currentStackSize >=
                                                                               incomingStackSize);

        public void SetItem(Item newItem, int incomingStackSize = 1)
        {
            item = newItem;

            if (!item)
            {
                _currentStackSize = 0;
                _stackSize = 0;
                return;
            }

            if (item.ItemStats == newItem.ItemStats)
            {
                _currentStackSize += incomingStackSize;
                return;
            }

            _currentStackSize = incomingStackSize;
            _stackSize = item.ItemStats.ItemLimit;
        }

        public bool TakeItem(int amount)
        {
            if (_currentStackSize < amount) return false;
            _currentStackSize -= amount;
            if (_currentStackSize == 0)
            {
                _currentStackSize = 0;
                _stackSize = 0;
                item = null;
            }
            return true;
        }

        public Item CurrentItem => item;
    }
}