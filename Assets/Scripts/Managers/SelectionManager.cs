using System;
using Common.Extensions;
using Game.Characters.World;
using Multiplayer.GameObjects;
using UI;
using UI.Images;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class SelectionManager : MonoBehaviour, ICharacterSelector
    {
        public static SelectionManager Instance { get; private set; }
        [SerializeField] private PenguinSelector[] penguinSelectors;
        [Header("UI")] [SerializeField] private GradientFillImage gradientFillImage;

        private GameControls _controls;
        private int _curIndex = 0;
        private bool _selectionClosed;

        // Kept for any existing listeners; the sequence uses LocalCharacterChosen.
        public event Action<GenericCharacter> OnCharacterSelected;

        // ICharacterSelector: prefab-id of the locally chosen character.
        public event Action<ulong> LocalCharacterChosen;

        // ICharacterSelector: prefab-id of whatever selector is currently highlighted.
        // The sequence reads this at window close to auto-assign a non-picker the
        // character they were last looking at. 0 if nothing valid is highlighted.
        public ulong CurrentHoverPrefabId
        {
            get
            {
                if (penguinSelectors == null || penguinSelectors.Length == 0) return 0;
                if (_curIndex < 0 || _curIndex >= penguinSelectors.Length) return 0;
                return penguinSelectors[_curIndex].PrefabId;
            }
        }

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            CharacterSelector.Active = this;

            _controls = new();
            _controls.UI.Enable();
            _controls.UI.Navigate.performed += OnNavigate;
            _controls.UI.Submit.performed += OnSubmit;

            Navigate(0);
        }

        private void OnNavigate(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => Navigate(ctx.ReadValue<Vector2>().x);

        private void OnSubmit(UnityEngine.InputSystem.InputAction.CallbackContext _)
            => SelectCurPenguin();

        public void SelectCurPenguin()
        {
            if (_selectionClosed) return; // window is over; ignore late input.

            var selector = penguinSelectors[_curIndex];
            var character = selector.Character;

            // Don't fire if this character is already taken by someone else.
            if (selector.IsTaken)
            {
                Debug.Log("[SelectionManager] Character already taken.");
                return;
            }

            selector.ChooseCharacter();
            OnCharacterSelected?.Invoke(character);

            var netObj = character.GetComponent<NetworkObject>();
            if (netObj) LocalCharacterChosen?.Invoke(netObj.PrefabIdHash);
            else Debug.LogError("[SelectionManager] Character has no NetworkObject.", character);
        }

        public void SetCurrentFill(float fill)
        {
            gradientFillImage.SetFill(fill);
        }

        // ICharacterSelector: driven every frame from the server-synced timer.
        // normalized is 1 -> 0 time remaining, mapped straight onto the fill.
        public void SetTimeRemaining(float normalized)
        {
            SetCurrentFill(normalized);
        }

        // ICharacterSelector: window closed on every peer. Lock input; the bar is
        // already empty. Spawning happens in the next sequence step.
        public void SelectionFinished()
        {
            if (_selectionClosed) return;
            _selectionClosed = true;
            _controls.UI.Disable();
            SetCurrentFill(0f);
        }

        // ICharacterSelector: server rejected our pick (race: taken first).
        public void OnSelectionRejected(ulong prefabId)
        {
            Debug.Log($"[SelectionManager] Selection {prefabId} rejected; re-open navigation.");
            // Re-enable input / play a buzzer here if desired; input is still live
            // unless the window already closed.
        }

        private void Navigate(float f)
        {
            if (_selectionClosed) return;

            penguinSelectors[_curIndex].Deselect();
            int dir = f.NormalizeToInt();
            _curIndex += dir;
            if (_curIndex >= penguinSelectors.Length) _curIndex = 0;
            else if (_curIndex < 0) _curIndex = penguinSelectors.Length - 1;

            penguinSelectors[_curIndex].Select();
        }

        private void OnDestroy()
        {
            if (CharacterSelector.Active == (ICharacterSelector)this)
                CharacterSelector.Active = null;

            if (_controls != null)
            {
                _controls.UI.Navigate.performed -= OnNavigate;
                _controls.UI.Submit.performed -= OnSubmit;
                _controls.UI.Disable();
                _controls.Dispose();
            }
        }
    }
}