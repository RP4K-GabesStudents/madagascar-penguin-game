using System;
using Interfaces;
using Managers;
using NUnit.Framework.Internal.Execution;
using Scriptable_Objects;
using UnityEngine;

namespace Inventory
{
    public class Item : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemStats itemStats;
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
