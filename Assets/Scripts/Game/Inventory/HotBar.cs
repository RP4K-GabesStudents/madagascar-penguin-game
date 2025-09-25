using Game.Characters.CapabilitySystem.Capabilities;
using UnityEngine;
using Utilities;

namespace Inventory
{
    public class HotBar : MonoBehaviour
    {
        private Itemslot _selectedItem;
        public int SelectedItemIndex => _curScrollIndex;
        
        private int _curScrollIndex = 0;
        
        private InventoryCapability _currentCapability; // What it reads from.

        [SerializeField] private Itemslot prefab;
        [SerializeField] protected Itemslot[] itemSlots;
        
        
        private void Start()
        {
            foreach (var jeff in itemSlots)
            {
                jeff.RemoveItem();
            }
            _selectedItem = itemSlots[_curScrollIndex];
            _selectedItem.MarkSelected();
        }
        
        public void OnSlotUpdated(int index)
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
            _curScrollIndex += index;
            if (_curScrollIndex >= itemSlots.Length)
            {
                _curScrollIndex = 0;
            }
            else if (_curScrollIndex < 0)
            {
                _curScrollIndex = itemSlots.Length - 1;   
            }
            UpdateScrollIndex(_curScrollIndex);
        }
    }
}