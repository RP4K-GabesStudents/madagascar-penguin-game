
using Interfaces;
using Managers;

using Scriptable_Objects;
using UnityEngine;

namespace Inventory
{
    public class Item : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemStats itemStats;
        public ItemStats ItemStats => itemStats;
        private MeshRenderer[] _meshRenderers;

        private void Awake()
        {
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        public void OnInteract()
        {
            gameObject.SetActive(false);
        }

        public HoverInfoStats GetHoverInfoStats()
        {
            return itemStats;
        }

        public bool CanHover()
        {
            return true;
        }

        public void OnHover()
        {
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.sharedMaterials = new[] { meshRenderer.sharedMaterials[0], ResourceManager.Instance.HoverMaterial };
            }
        }

        public void OnHoverEnd()
        {
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.sharedMaterials = new[] { meshRenderer.sharedMaterials[0] };
            }
        }
    }
    
}
