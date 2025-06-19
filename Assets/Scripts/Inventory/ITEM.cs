
using Interfaces;
using Managers;

using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;

namespace Inventory
{
    public class Item : NetworkBehaviour, IInteractable
    {
        [SerializeField] private ItemStats itemStats;
        public ItemStats ItemStats => itemStats;
        private MeshRenderer[] _meshRenderers;
        private NetworkObject _networkObject;

        private void Awake()
        {
            _networkObject = GetComponent<NetworkObject>();
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        public void OnInteract()
        {
            //gameObject.SetActive(false);
            Interact_ServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void Interact_ServerRpc()
        {
            _networkObject.Despawn(false);
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
