using Inventory;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// A PlayerController is the mediator pattern and represents the local player object.
    /// When the player spawns in, they are automatically given a PlayerController
    /// From here, we spawn in the components they'll need.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        private GenericCharacter _currentCharacter;
        private HotBar _hotBar;
        private GameControls _gameControls ;

        private void Start()
        {
            _hotBar = GetComponent<HotBar>();
            IInputSubscriber[]  inputSubscribers = GetComponentsInChildren<IInputSubscriber>();
            
            Debug.Log("Binding controls to: " + inputSubscribers.Length + " controllers.");

            _gameControls ??= new GameControls();
            
            foreach (IInputSubscriber comp in inputSubscribers)
            {
                comp.BindControls(_gameControls);
            }
        }

        public void OnEnable()
        {
            _currentCharacter ??= GetComponent<GenericCharacter>();
            if (!_currentCharacter.IsOwner) return;
            _gameControls ??= new GameControls();
            EnableGame();
        }

        public void OnDisable()
        {
            if (!_currentCharacter.IsOwner) return;
            EnableUi();
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