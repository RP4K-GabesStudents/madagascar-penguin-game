using Interfaces;
using Managers;
using penguin;
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
        protected PlayerController _oner;

        private void Awake()
        {
            _networkObject = GetComponent<NetworkObject>();
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        public void OnInteract(PlayerController oner)
        {
            //gameObject.SetActive(false);
            SetOwner(oner);
            Interact_ServerRpc();
        }
        public void SetOwner(PlayerController oner)
        {
            _oner = oner;
        }

        [ServerRpc(RequireOwnership = false)]
        private void Interact_ServerRpc(ServerRpcParams id = default)
        {
            Hide_ClientRpc();
            _networkObject.ChangeOwnership(id.Receive.SenderClientId);
            
            //_networkObject.TrySetParent(id.Receive. )
        }

        [ClientRpc]
        private void Hide_ClientRpc()
        {
            gameObject.SetActive(false);
        }

        [ClientRpc]
        private void Show_ClientRpc()
        {
            
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

        public virtual void UseItem()
        {
            
        }
    }
    
}
