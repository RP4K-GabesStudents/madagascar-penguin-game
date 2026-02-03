using System;
using UnityEngine;

namespace UI.DragAndDrop
{
    public class DragAndDropZone : MonoBehaviour
    {
       [SerializeField] private RectTransform rectTransform;
       
        private RectTransform _dragTransform;
        
        private DragAndDropObject _dragAndDropObject;
        private static DragAndDropZone _current;
        public event Func<DragAndDropObject,bool> AcceptRules;
        public event Action<DragAndDropObject> OnGain;
        public event Action<DragAndDropObject> OnLost;

        private bool _isValidTarget;
        
        private void Awake()
        {
            gameObject.isStatic = true;
            OnItemChanged(null);
            DragAndDropObject.OnDragItemChanged += OnItemChanged;

        }


        private void OnDestroy()
        {
            DragAndDropObject.OnDragItemChanged -= OnItemChanged;
        }

        private void OnItemChanged(DragAndDropObject obj)
        {
            _isValidTarget = obj && (obj.GetLayers() & 1<<gameObject.layer) != 0;
            _dragAndDropObject = obj;
            if(_isValidTarget) _dragTransform = ((RectTransform)obj.transform);
        }

        private void Update()
        {
            if (!_isValidTarget) return;
            
            if (_current == this)
            {
                //Inverse
                if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, _dragTransform.position))
                {
                    _current = null;
                    _dragAndDropObject.UnmarkTarget();
                }
            }
            else
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, _dragTransform.position))
                {
                    _current = this;
                    _dragAndDropObject.MarkTarget(this);
                }
            }
        }

        public bool CanAcceptItem(DragAndDropObject obj)
        {
            if (AcceptRules == null) return true;
            return AcceptRules.Invoke(obj);
        }

        public Vector2 PlaceNearestLocation(Vector2 startLocation)
        {
            // Get the RectTransform's size and pivot
            Vector2 location = rectTransform.position;
            Vector2 min = rectTransform.rect.min + location;
            Vector2 max = rectTransform.rect.max + location;

            // Clamp the input position to the bounds
            float x = Mathf.Clamp(startLocation.x, min.x, max.x);
            float y = Mathf.Clamp(startLocation.y, min.y, max.y);

            // Return the clamped position in local space
            return new Vector2(x, y);
        }

        public void OnItemGained(DragAndDropObject obj)
        {
            OnGain?.Invoke(obj);
        }
        public void OnItemLost(DragAndDropObject obj)
        {
            OnLost?.Invoke(obj);
        }
    }
}
