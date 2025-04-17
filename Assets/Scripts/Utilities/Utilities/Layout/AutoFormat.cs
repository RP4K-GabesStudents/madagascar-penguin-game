using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Utilities.Layout
{
    [RequireComponent(typeof(LayoutGroup))]
    [ExecuteAlways]
    public class AutoFormat : MonoBehaviour
    {
        private VerticalLayoutGroup _vertical;
        private HorizontalLayoutGroup _horizontal;
        private RectTransform _rectTransform;
        [SerializeField] private float manualSpacing;

        private void Awake()
        {
            _vertical = GetComponent<VerticalLayoutGroup>();
            _horizontal = GetComponent<HorizontalLayoutGroup>();
            _rectTransform = transform as RectTransform;
        }
        private void OnTransformChildrenChanged()
        {
            if (_vertical) UpdateVertically();
            else if (_horizontal) UpdateHorizontally();
        }

        private void UpdateHorizontally()
        {

            if (_horizontal.childControlWidth)
            {
                Debug.LogWarning("Will not auto update with childControlWidth enabled");
                return;
            }
            
            float spacing = (manualSpacing!=0?manualSpacing:_horizontal.spacing) * 2;
            float padding = _horizontal.padding.left + _horizontal.padding.right;
            int count = transform.childCount;


            Vector2 target = new Vector2((count - 1) * spacing + padding, _rectTransform.sizeDelta.y);

            for (int i = 0; i < count; i++)
            {
                target.x += ((RectTransform)transform.GetChild(i)).sizeDelta.x;
            }
            //_rectTransform.position = 

            _rectTransform.sizeDelta = target;
        }

        private void UpdateVertically()
        {
            if (_vertical.childControlHeight)
            {
                Debug.LogWarning("Will not auto update with childControlHeight enabled");
                return;
            }
            
            float spacing = (manualSpacing!=0?manualSpacing:_vertical.spacing) * 2;
            float padding = _vertical.padding.top + _vertical.padding.bottom;
            int count = transform.childCount;
            
            //45


            Vector2 target = new Vector2(_rectTransform.sizeDelta.x,(count - 1) * spacing + padding);

            for (int i = 0; i < count; i++)
            {
                target.y += ((RectTransform)transform.GetChild(i)).sizeDelta.y;
            }

            _rectTransform.sizeDelta = target;
        }
    }
}
