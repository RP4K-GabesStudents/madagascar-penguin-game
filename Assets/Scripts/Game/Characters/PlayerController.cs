using System;
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
        private GameControls _gameControls ;

        public static PlayerController Instance { get; private set; }
        
        private void Awake()
        {
           if (Instance && Instance != this)
           {
               Destroy(this);
               return;
           }
           Instance = this;
           DontDestroyOnLoad(gameObject);
        }

        public void SubscribeTo(GameObject obj)
        {
            IInputSubscriber[]  inputSubscribers = obj.GetComponentsInChildren<IInputSubscriber>();
            
            Debug.Log("Binding controls to: " + inputSubscribers.Length + " controllers.");

            _gameControls ??= new GameControls();
            
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
    }
}