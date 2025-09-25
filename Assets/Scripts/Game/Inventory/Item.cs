using Game.Characters;
using Game.Objects;
using Inventory;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;

namespace Game.Inventory
{
    [SelectionBase, RequireComponent(typeof(Highlight))]
    public class Item : NetworkBehaviour, IInteractable
    {
        [SerializeField] private ItemStats itemStats;
        public ItemStats ItemStats => itemStats;
        protected GenericCharacter _oner;
        protected Highlight _highlight;

        protected virtual void Awake()
        {
            _highlight =  GetComponent<Highlight>();
        }

        public void OnInteract(GenericCharacter oner)
        {
            //gameObject.SetActive(false);
            SetOwner(oner);
            Interact_ServerRpc();
        }
        public void SetOwner(GenericCharacter oner)
        {
            _oner = oner;
        }

        [ServerRpc(RequireOwnership = false)]
        private void Interact_ServerRpc(ServerRpcParams id = default)
        {
            Hide_ClientRpc();
            ulong user = id.Receive.SenderClientId;
            
            NetworkObject.ChangeOwnership(user);
            
            Debug.LogWarning("We don't actually know who the owning object is though (for reparenting)");
            //NetworkObject.TrySetParent(user);
        }

        [ClientRpc]
        private void Hide_ClientRpc()
        {
            gameObject.SetActive(false);
        }

        [ClientRpc]
        private void Show_ClientRpc()
        {
            gameObject.SetActive(true);
        }

        public HoverInfoStats GetHoverInfoStats() => itemStats;
        public bool CanHover() => !_oner;

        public void OnHover() => _highlight.enabled = true;

        public void OnHoverEnd() => _highlight.enabled = false;

        public virtual bool CanBeUsed() => _oner != null;

        public bool TryUseItem()
        {
            if (!CanBeUsed()) return false;
            UseItem();
            return true;
        }

        public virtual void UseItem() { Debug.Log("Used an item which does nothing: ", gameObject); }


        public void StartUsing()
        {
            
        }

        public void StopUsing()
        {
            
        }
    }
    
}
