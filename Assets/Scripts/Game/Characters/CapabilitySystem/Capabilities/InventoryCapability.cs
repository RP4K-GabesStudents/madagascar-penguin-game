using System;
using Game.Inventory;
using Inventory;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    public class InventoryCapability : NetworkBehaviour, IInputSubscriber
    {
        public Item currentSelectedItem => items[_hotBar.SelectedItemIndex].currentItem;
        
        private GameControls _reference; // Temporary solution...
        
        private PlayerController _player;
        
        private HotBar _hotBar; // It really shouldn't work this way.

        [SerializeField] private Transform parent;

        [SerializeField] private ItemContainer[] items = new ItemContainer[10];
        

        #region Controls
        public void BindControls(GameControls controls)
        {
            if (NetworkManager.IsConnectedClient && !NetworkObject.IsOwner)
            {
                Debug.LogError("THIS IS NOT US???");
                return;
            }
            //Ideally we want to do this IN EVERY OBJECT THAT IS IINPUTSUBSCRIBER....
            
            Unsubscribe();
            
            _reference = controls;
            
            Subscribe();
        }
        private void Subscribe()
        {
            if (_reference == null) return;
     
            _reference.Player.Attack.performed += ChangeUsingState;
            _reference.Player.HotBarScroll.performed += ScrollHotBar;
            _reference.Player.HotBarSlot1.performed += SelectSlotOne;
            _reference.Player.HotBarSlot2.performed += SelectSlotTwo;
            _reference.Player.HotBarSlot3.performed += SelectSlotThree;
            _reference.Player.HotBarSlot4.performed += SelectSlotFour;
            _reference.Player.HotBarSlot5.performed += SelectSlotFive;
            
            Debug.LogError("Austin do dropping >:)");
            Debug.LogError("Austin Try figure out, HOW DO WE GET THE HOTBAR TO BE NOT NULL... If you do wanna try, you need to CREATE THE HOTBAR ITEM SLOTS from the code... You need to make a loop");
        }
        private void Unsubscribe()
        {
            if (_reference == null) return;
            
            _reference.Player.Attack.performed -= ChangeUsingState;
            _reference.Player.HotBarScroll.performed -= ScrollHotBar;
            _reference.Player.HotBarSlot1.performed -= SelectSlotOne;
            _reference.Player.HotBarSlot2.performed -= SelectSlotTwo;
            _reference.Player.HotBarSlot3.performed -= SelectSlotThree;
            _reference.Player.HotBarSlot4.performed -= SelectSlotFour;
            _reference.Player.HotBarSlot5.performed -= SelectSlotFive;
        }
        private void OnEnable()
        {
            Subscribe();
        }
        private void OnDisable()
        { 
            Unsubscribe();
        }
        #endregion
        
        private void SelectSlotOne(InputAction.CallbackContext obj) => SelectHotBar(0);
        private void SelectSlotTwo(InputAction.CallbackContext obj) => SelectHotBar(1);
        private void SelectSlotThree(InputAction.CallbackContext obj) => SelectHotBar(2);
        private void SelectSlotFour(InputAction.CallbackContext obj) => SelectHotBar(3);
        private void SelectSlotFive(InputAction.CallbackContext obj) => SelectHotBar(4);
        
        private void ChangeUsingState(InputAction.CallbackContext obj)
        {
            if (!currentSelectedItem) return;
            
            bool state = obj.ReadValueAsButton();

            if (state) currentSelectedItem.StartUsing();
            else currentSelectedItem.StopUsing();

        }

        private void ScrollHotBar(InputAction.CallbackContext obj)
        {
            int direction = obj.ReadValue<int>();

            if (currentSelectedItem) currentSelectedItem.Hide_ClientRpc();
            
            _hotBar.UpdateScrollSlot(direction);
            
            if(currentSelectedItem) currentSelectedItem.Show_ClientRpc();
        }
        
        private void SelectHotBar(int index)
        {
            if (currentSelectedItem) currentSelectedItem.Hide_ClientRpc();
            
            _hotBar.UpdateScrollIndex(index);
            
            if(currentSelectedItem) currentSelectedItem.Show_ClientRpc();

        }

        private void DropItem(int slot)
        {
            var item = items[slot];
            if (item == null) return;
            
            //Austin fill this out...
            
            Debug.LogError("Austin do dropping >:)");
        }

        public void TryPickup(Item item, int stackSize = 1)
        {
            //Attach the item to our mouth, place it in the correct spot... When can we NOT pick an object?
            item.transform.SetParent(parent, false);

            //iterate through the inventory, choose left most spot
            for (var i = 0; i < items.Length; i++)
            {
                var itemContainer = items[i];
                if (itemContainer.CanAcceptItem(item, stackSize)) // Try accept the stack size...
                {
                    itemContainer.SetItem(item, stackSize);
                    return;
                }
            }
            
            //If we fail... We drop what item?
            DropItem(_hotBar.SelectedItemIndex);
        } 
    }

    [Serializable]
    public class ItemContainer
    {
        private int _currentStackSize;
        private int _stackSize;
        [SerializeField] private Item item;
        
        public void ReInit()
        {
            if (!item) return;
            _currentStackSize = 1;
            _stackSize = item.ItemStats.ItemLimit;
        }

        public bool CanAcceptItem(Item newItem, int incomingStackSize = 1) => !item || (newItem.ItemStats == item.ItemStats && _stackSize - _currentStackSize >= incomingStackSize);
        public void SetItem( Item newItem, int incomingStackSize = 1)
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
        public Item currentItem => item;
    }
}