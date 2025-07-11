using Interfaces;
using Managers;
using penguin;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;

namespace Inventory
{
    [SelectionBase]
    public class Item : NetworkBehaviour, IInteractable
    {
        [SerializeField] private ItemStats itemStats;
        public ItemStats ItemStats => itemStats;
        private MeshRenderer[] _meshRenderers;
        protected PlayerController _oner;

        protected virtual void Awake()
        {
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
            ulong user = id.Receive.SenderClientId;
            
            NetworkObject.ChangeOwnership(user);
            NetworkObject.TrySetParent(user);
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

        public HoverInfoStats GetHoverInfoStats() => itemStats;
        public bool CanHover() => !_oner;

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
        
        public virtual bool CanBeUsed()
        {
            return _oner != null;
        }

        public bool UseItem()
        {
            if (!CanBeUsed()) return false;
            return true;
        }
        
        
        
    }
    
}
