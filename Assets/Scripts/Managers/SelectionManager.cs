using System;
using System.Threading.Tasks;
using Game.Characters;
using UI;
using UnityEngine;
using Utilities.Utilities.General;


namespace Managers
{
    public class SelectionManager : MonoBehaviour
    {
        public static SelectionManager Instance { get; private set; }
        [SerializeField] private PenguinSelector[] penguinSelectors;
        private GameControls _controls;
        private int _curIndex = 0;
        public event Action<GenericCharacter> OnCharacterSelected; 
        
        private void Awake()
        {
            
            if (Instance && Instance != this)
            {
               Destroy(gameObject);
               return;
            }
            Instance = this;
            
            _controls = new ();
            _controls.UI.Enable();
            _controls.UI.Navigate.performed += ctx => Navigate(ctx.ReadValue<Vector2>().x);
            _controls.UI.Submit.performed += _ => SelectCurPenguin();

            penguinSelectors[0].Select();
        }

        private void SelectCurPenguin()
        {
            penguinSelectors[_curIndex].ChooseCharacter();
            OnCharacterSelected?.Invoke(penguinSelectors[_curIndex].Character);
        }

        private void Navigate(float f)
        {
            penguinSelectors[_curIndex].Deselect();
            int dir = f.NormalizeToInt();
            _curIndex += dir;
            if (_curIndex >= penguinSelectors.Length) _curIndex = 0;
            else if (_curIndex < 0) _curIndex = penguinSelectors.Length - 1;
            
            penguinSelectors[_curIndex].Select();
        }
        

        private void OnDestroy()
        { 
            _controls.UI.Disable();
            _controls.Dispose();
        }
    }
}
