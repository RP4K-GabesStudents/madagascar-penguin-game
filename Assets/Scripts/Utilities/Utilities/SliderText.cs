using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities.Utilities
{
    public class SliderText : MonoBehaviour
    {
        private enum ESliderType
        {
            Percentage,
            Number,
            NumberWithMax,
            Hidden
        }

        [SerializeField] private ESliderType sliderType;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private bool inverse;
        private float _maxValue;
        private float _currentValue;


        private void OnEnable()
        {
            UpdateMax(slider.maxValue);
            slider.onValueChanged.AddListener(UpdateCurrent);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(UpdateCurrent);
        }

        public void UpdateMax(float value)
        {
            _maxValue = value;
            UpdateCurrent(_currentValue);
        }

        public void UpdateCurrent(float value)
        {
            _currentValue = value;
        
            float percent = _currentValue / _maxValue;
            if(inverse) percent = 1 - percent;
            slider.value = percent;

            switch (sliderType)
            {
                case ESliderType.Percentage:
                    text.text = ((int)(percent * 100)) + "%";
                    break;
                case ESliderType.Number:
                    text.text = _currentValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case ESliderType.NumberWithMax:
                    text.text = _currentValue.ToString(CultureInfo.InvariantCulture) + "/" + _maxValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case ESliderType.Hidden:
                    text?.gameObject.SetActive(false);
                    break;
            }

        }

    }
}
