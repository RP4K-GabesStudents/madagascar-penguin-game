using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utilities.Hover
{
    [RequireComponent(typeof(RectTransform))]
    public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Vector2 hoverSize = Vector2.zero;
        [SerializeField] private bool useLiteralScale;
        [SerializeField] private float transitionDuration;
        [SerializeField] private AnimationCurve transitionCurve;

        private Vector2 _originalScale;
        private RectTransform  _rectTransform;
        private Coroutine _hoverCoroutine;

        private float _currentTransitionTime;

        private bool _isLocked;
        private void Awake()
        {
            _rectTransform ??= transform as RectTransform;
            if(_rectTransform == null) throw new UnityException("Missing component of RectTransform");
            _originalScale = useLiteralScale?_rectTransform.localScale:_rectTransform.sizeDelta;
        }
        

        public void OnPointerEnter(PointerEventData eventData)
        {
            Grow();   
        }

  
        public void OnPointerExit(PointerEventData eventData)
        {         
          Shrink();
        }

        public void Grow(bool byPass = false)
        {
            if (_isLocked && !byPass) return;
            if (_hoverCoroutine != null)
            {
                _currentTransitionTime = transitionDuration - _currentTransitionTime;
                StopCoroutine(_hoverCoroutine);
            }
            _hoverCoroutine = StartCoroutine(Transition(useLiteralScale?_rectTransform.localScale:_rectTransform.sizeDelta, hoverSize));
        }

        public void Shrink(bool byPass = false)
        {
            
            if (_isLocked && !byPass) return;
             
            if (_hoverCoroutine != null)
            {
                _currentTransitionTime = transitionDuration - _currentTransitionTime;
                StopCoroutine(_hoverCoroutine);
            }
            _hoverCoroutine = StartCoroutine(Transition(useLiteralScale?_rectTransform.localScale:_rectTransform.sizeDelta, _originalScale));
        }

        private IEnumerator Transition(Vector2 start, Vector2 end)
        {
            if (useLiteralScale)
            {
                Vector3 startScale = new Vector3(start.x, start.y, 1);
                Vector3 endScale = new Vector3(end.x, end.y, 1);
                while (_currentTransitionTime < transitionDuration)
                {
                    _rectTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, transitionCurve.Evaluate(_currentTransitionTime / transitionDuration));
                    _currentTransitionTime += Time.deltaTime;
                    yield return null;
                }

                _rectTransform.localScale = endScale;
            }
            else
            {
                while (_currentTransitionTime < transitionDuration)
                {
                    _rectTransform.sizeDelta = Vector2.LerpUnclamped(start, end, transitionCurve.Evaluate(_currentTransitionTime / transitionDuration));
                    _currentTransitionTime += Time.deltaTime;
                    yield return null;
                }

                _rectTransform.sizeDelta = end;

            }

            _currentTransitionTime = 0;
            _hoverCoroutine = null;
        }

        public void Lock(bool state)
        {
            _isLocked = state;
        }
    }
}
