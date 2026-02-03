using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI.DragAndDrop
{
    public class DragAndDropObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private LayerMask targetLayers;
        
        private Canvas _canvas;

        private bool _isBeingHeld;

        public UnityEvent onHover;
        public UnityEvent onStopHover;
        public UnityEvent onNewTarget;
        public UnityEvent onTargetRemoved;
        public UnityEvent<DragAndDropZone,DragAndDropZone> onApplyNewTarget;
        
        public DragAndDropZone CurrentTarget { get; private set; }
        public DragAndDropZone OldTarget  { get; private set; }
        
        public static event Action<DragAndDropObject> OnDragItemChanged;
        
        
        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            OldTarget = transform.parent.GetComponent<DragAndDropZone>();
        }


        private void Update()
        {
            if (!_isBeingHeld) return;
            //RectTransformUtility.screenpoint
            transform.position = Pointer.current.position.ReadValue();
        }
        
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isBeingHeld) return;
            onHover?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isBeingHeld) return;
            onStopHover?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isBeingHeld = true;
            transform.SetParent(_canvas.transform);
            OnDragItemChanged?.Invoke(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isBeingHeld = false;
            OnDragItemChanged?.Invoke(null); // There is no longer a current object
            
            if (CurrentTarget && CurrentTarget.CanAcceptItem(this))
            {
                SetParent(CurrentTarget);
                CurrentTarget = null;
            }
            else
            {
                transform.SetParent(OldTarget.transform);
                transform.position = OldTarget.PlaceNearestLocation(transform.position);
            }
        }

        public void SetParent(DragAndDropZone target)
        {
            OldTarget.OnItemLost(this);
            target.OnItemGained(this);

            transform.SetParent(target.transform);
            onApplyNewTarget?.Invoke(OldTarget, target);
            OldTarget = target;

        }

        public int GetLayers()
        {
            return targetLayers;
        }

        public void UnmarkTarget()
        {
            CurrentTarget = null;
            onTargetRemoved?.Invoke();
        }

        public void MarkTarget(DragAndDropZone newTarget)
        {
            onNewTarget?.Invoke();
            if (OldTarget == newTarget) return; // This is the same object
            CurrentTarget = newTarget;
        }

        private void OnTransformParentChanged()
        {
            if (transform.parent.TryGetComponent(out DragAndDropZone target) && target != CurrentTarget)
            {
                CurrentTarget = null;
                SetParent(target);
            }
        }
    }
}
