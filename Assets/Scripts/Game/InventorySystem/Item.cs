using System.Collections;
using Game.Characters.CapabilitySystem.Capabilities;
using Game.Characters.World;
using Game.Objects;
using Managers;
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
        private static int _excludedLayer;

        private Rigidbody _rb;

        protected virtual void Awake()
        {
            _excludedLayer = LayerMask.NameToLayer("Player");
            _highlight = GetComponent<Highlight>();
            _rb =  GetComponent<Rigidbody>();
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
            ExcludePlayer_ClientRpc(StaticUtilities.PlayerLayer);
            ulong user = id.Receive.SenderClientId;
            NetworkObject.ChangeOwnership(user);
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
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

        [ClientRpc]
        private void ExcludePlayer_ClientRpc(int layer)
        {
            _rb.excludeLayers = layer;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
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

        public void AttachTo(NetworkObject parent,  bool resetPos = true, bool resetRot = true, bool resetScale = false)
        {

            AttachTo_ServerRpc(parent.NetworkObjectId, resetScale, resetPos, resetRot);
        }
        
        
        [ServerRpc(RequireOwnership = false)]
        public void AttachTo_ServerRpc(ulong id, bool resetScale, bool resetPos, bool resetRot)
        {
            AttachTo_ClientRpc(id, resetScale,  resetPos, resetRot);
        }
    
        [ClientRpc]
        private void AttachTo_ClientRpc(ulong id, bool resetScale, bool resetPos, bool resetRot)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject attachRoot)){
                Debug.LogError("Couldn't attach to target object, " , gameObject);
                return;
            }
            
            InventoryCapability capability = attachRoot.GetComponentInChildren<InventoryCapability>();
            transform.SetParent(capability.Parent, !resetScale);
            if (resetPos && resetRot)  transform.SetLocalPositionAndRotation(Vector3.zero,   Quaternion.identity);
            else if (resetPos) transform.localPosition = Vector3.zero;
            else if (resetRot) transform.localRotation = Quaternion.identity;
        }

        public void Drop()
        {
            Debug.Log("ITEM: Drop");
            Drop_ServerRpc();
        }

        [ServerRpc]
        public void Drop_ServerRpc()
        {
            Debug.Log("ITEM: Drop server");
            Vector3 forward = transform.parent.forward;
            Drop_ClientRpc();
            
            transform.parent = null;
            _rb.useGravity = true;
            _rb.constraints = RigidbodyConstraints.None;
            _rb.AddForce(forward * Random.Range(0, 3), ForceMode.Impulse);
            _rb.AddTorque(Random.insideUnitSphere * 10, ForceMode.Impulse);
            
            Debug.DrawRay(transform.position, forward * 10, Color.red);
        }

        [ClientRpc]
        public void Drop_ClientRpc()
        {
            Debug.Log("ITEM: Drop Client");
            transform.parent = null;
        }
    }
    
}
