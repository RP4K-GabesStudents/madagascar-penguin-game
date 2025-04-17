using System;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class ToggleSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;

        public Action<bool> OnValueChanged { get; set; }

        private bool _state;
        public bool State {
            get => _state;
            set
            {
                _state = value;
                slider.value = State ? slider.maxValue : slider.minValue;
                OnValueChanged?.Invoke(_state);
            }
        }

        public void OnPress()
        {
            State = !State;
        }
    }
}
