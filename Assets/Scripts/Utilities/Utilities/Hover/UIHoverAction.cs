using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Utilities.Hover
{
    [RequireComponent(typeof(RectTransform))]
    public class UIHoverAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UnityEvent onHover;
        [SerializeField] private UnityEvent onHoverExit;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            onHover?.Invoke();
        }

  
        public void OnPointerExit(PointerEventData eventData)
        {
            onHoverExit?.Invoke();
        }

    }
}
