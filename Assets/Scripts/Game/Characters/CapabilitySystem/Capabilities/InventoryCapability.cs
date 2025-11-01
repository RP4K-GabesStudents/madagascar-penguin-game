using System;
using Game.InventorySystem;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;
using Utilities.Utilities.General;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    public class InventoryCapability : MonoBehaviour, IInputSubscriber
    {
        public Item CurrentSelectedItem => _inventory.items[_hotBar.SelectedItemIndex].CurrentItem;
        private GameControls _reference; // Temporary solution...
        private PlayerController _player;
        private HotBar _hotBar;
        private Inventory _inventory;
        [SerializeField] private Transform parent;
        [SerializeField] private HotBar hotBarPrefab; //fill this out later 10/08/25
        private NetworkObject rootParent;
        
        public Transform Parent => parent;


        private void Awake()
        {
            rootParent = transform.root.GetComponent<NetworkObject>();
        }

        #region Controls

        public void BindControls(GameControls controls)
        {
            _inventory = GetComponentInParent<Inventory>();
            _hotBar = Instantiate(hotBarPrefab);
            _hotBar.SetTargetInventory(_inventory);
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

            Debug.LogError("Austin do dropping >:). I tried");
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
            if (!CurrentSelectedItem) return;

            bool state = obj.ReadValueAsButton();

            if (state) CurrentSelectedItem.StartUsing();
            else CurrentSelectedItem.StopUsing();
        }
        private void ScrollHotBar(InputAction.CallbackContext obj)
        {
            int direction = obj.ReadValue<float>().NormalizeToInt();

            if (CurrentSelectedItem) CurrentSelectedItem.Hide_ClientRpc();

            _hotBar.UpdateScrollSlot(direction);

            if (CurrentSelectedItem) CurrentSelectedItem.Show_ClientRpc();
        }
        private void SelectHotBar(int index)
        {
            if (CurrentSelectedItem) CurrentSelectedItem.Hide_ClientRpc();

            _hotBar.UpdateScrollIndex(index);

            if (CurrentSelectedItem) CurrentSelectedItem.Show_ClientRpc();
        }
        private void DropItem(int slot)
        {
            _inventory.DropItem(slot);
        }
        public void TryPickup(Item item, int stackSize = 1)
        {
            int index = _inventory.TryPickup(item, stackSize);
            if (index == -1)
            {
                if (Settings.GamePlaySettings.dropIfInventoryFull)
                {
                    Debug.Log("does not handle dropping full stacks");
                    DropItem(index);
                    TryPickup(item, stackSize);
                }

                return;
            }


            //Attach the item to our mouth, place it in the correct spot... When can we NOT pick an object?
            item.AttachTo(rootParent, true, true, false);
            
            if (Settings.GamePlaySettings.autoEquip)
            {
                _hotBar.UpdateScrollIndex(index);
            }
        }
    }
}