using Game.Inventory;
using Inventory;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    public class InventoryCapability : NetworkBehaviour, IInputSubscriber
    {
        public Item currentSelectedItem { get; private set; }

        private GameControls _reference; // Temporary solution...
        
        private PlayerController _player;
        
        private HotBar _hotBar; // It really shouldn't work this way.
        
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
        }
        private void Unsubscribe()
        {
            if (_reference == null) return;
            
            _reference.Player.Attack.performed -= ChangeUsingState;
            _reference.Player.HotBarScroll.performed -= ScrollHotBar;
            _reference.Player.HotBarSlot1.performed += SelectSlotOne;
            _reference.Player.HotBarSlot2.performed += SelectSlotTwo;
            _reference.Player.HotBarSlot3.performed += SelectSlotThree;
            _reference.Player.HotBarSlot4.performed += SelectSlotFour;
            _reference.Player.HotBarSlot5.performed += SelectSlotFive;
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

            if (currentSelectedItem) currentSelectedItem.StopUsing();
            _hotBar.UpdateScrollSlot(direction);
            currentSelectedItem = _hotBar.SelectedItem;
        }
        
        private void SelectHotBar(int index)
        {
            if (currentSelectedItem) currentSelectedItem.StopUsing();
            _hotBar.UpdateScrollIndex(index);
            currentSelectedItem = _hotBar.SelectedItem;
        }
    }
}