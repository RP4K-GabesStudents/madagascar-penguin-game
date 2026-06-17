using UnityEngine;
using UnityEngine.UI;

namespace UI.Images
{
    [RequireComponent(typeof(Image))]
    public class GradientFillImage : MonoBehaviour
    {
        public Gradient gradient;


        private Image _image;
        private float _lastFill = -1f;

        private void Awake()
        {
            _image = GetComponent<Image>();
            Apply(_image.fillAmount);
        }

        private void OnDisable()
        {
            _lastFill = -1;
        }

        private void Update()
        {
            // Only runs while enabled, i.e. while polling. No per-frame branch needed.
            float fill = _image.fillAmount;
            if (!Mathf.Approximately(fill, _lastFill))
                Apply(fill);
        }

        private void Apply(float fill)
        {
            _lastFill = fill;
            // fillAmount is already normalized 0..1.
            _image.color = gradient.Evaluate(Mathf.Clamp01(fill));
        }

        public void SetFill(float fill)
        {
            _image.fillAmount = fill ;
            if (!Mathf.Approximately(fill, _lastFill))
                Apply(fill);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            _image ??= GetComponent<Image>();
            Apply(_image.fillAmount);
        }
#endif
    }
}