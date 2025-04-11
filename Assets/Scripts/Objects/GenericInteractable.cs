using Interfaces;
using Managers;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Objects
{
    public class GenericInteractable : MonoBehaviour, IInteractable
    {
        
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] public UnityEvent onHoverBegin;
        [SerializeField] public UnityEvent onHoverEnd;
        [SerializeField] public UnityEvent onInteracted;
        [SerializeField] private HoverInfoStats hoverInfoStats;
        public void OnInteract()
        {
            onInteracted.Invoke();
        }

        public HoverInfoStats GetHoverInfoStats()
        {
            return hoverInfoStats;
        }

        public void OnHover()
        {
            onHoverBegin.Invoke();
            meshRenderer.sharedMaterials = new[] { meshRenderer.sharedMaterials[0], ResourceManager.Instance.HoverMaterial };
            
        }

        public void OnHoverEnd()
        {
            onHoverEnd.Invoke();
            meshRenderer.sharedMaterials = new[] { meshRenderer.sharedMaterials[0] };
        }
    }
}