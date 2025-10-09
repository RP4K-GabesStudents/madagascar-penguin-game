using System.Collections;
using Game.Characters.World;
using Game.Objects;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;

namespace Game.InventorySystem
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
        }

        [ClientRpc]
        public void Hide_ClientRpc()
        {
            gameObject.SetActive(false);
            StopUsing();
        }

        [ClientRpc]
        public void Show_ClientRpc()
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

        public virtual void UseItem()
        {
            StartCoroutine(ActivateItem());
            Debug.Log("Used an item which does nothing: ", gameObject); 
        }

        private IEnumerator ActivateItem()
        {
            if (!TryUseItem()) yield break;
            
            yield return new WaitForSeconds(itemStats.CoolDownTime);
        }

        //Austin: make a Coroutine, or async/await to manage how an object works... Some can be used per click, others you can hold down... Do this how you want...
        //Should probably validate "TryUseItem" on NetworkVariables exclusively ( for safety reasons )
        public void StartUsing()
        {
            Debug.Log("ITEM: StartUsing", gameObject);
        }

        public void StopUsing()
        {
            Debug.Log("ITEM: StopUsing", gameObject);
        }
    }
    
}
