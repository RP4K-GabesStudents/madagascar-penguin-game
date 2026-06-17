using UnityEngine;
using UnityEngine.UI;

namespace UI.Sliders
{
    [RequireComponent(typeof(Slider))]
    public class GradientSlider : MonoBehaviour
    {
        public Gradient gradient;

        private Slider _slider;
        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(_slider.value);
        }

        private void OnValueChanged(float arg0)
        {
            var block = _slider.colors;
            block.normalColor = gradient.Evaluate((arg0 - _slider.minValue) / (_slider.maxValue - _slider.minValue));
            _slider.colors = block;
        }

        private void OnDestroy()
        {
            _slider?.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnDrawGizmosSelected()
        {
            _slider??= GetComponent<Slider>();
            OnValueChanged(_slider.value);
        }
    }
}
