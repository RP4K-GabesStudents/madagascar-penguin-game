using Inventory;
using Unity.Netcode;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// A PlayerController is the mediator pattern and represents the local player object.
    /// When the player spawns in, they are automatically given a PlayerController
    /// From here, we spawn in the components they'll need.
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        
        private GenericCharacter _currentCharacter;
        private HotBar _hotBar;
        private GameControls _gameControls = new();
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            
            IInputSubscriber[]  inputSubscribers = GetComponentsInChildren<IInputSubscriber>();
            foreach (IInputSubscriber comp in inputSubscribers)
            {
                comp.BindControls(_gameControls);
            }

            EnableGame();
        }
        
        public void EnableGame()
        {
            _gameControls.Player.Enable();
            _gameControls.UI.Disable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void EnableUi()
        {     
            _gameControls.Player.Disable();
            _gameControls.UI.Enable();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        /*
         * For enhanced security, we could handle inputs via NetworkVariable and force server validation before allowing processing.
         */
        
        public void SetSelected(int key) => _hotBar.UpdateScrollIndex(key);
        public void ScrollSelected(float scroll) => _hotBar.UpdateScrollSlot((int)scroll);
    }
}