using Game.Characters;
using Game.Objects;
using Interfaces;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;


namespace Objects
{
    [SelectionBase, RequireComponent(typeof(Highlight))]
    public class Chest : NetworkBehaviour, IInteractable
    {
        private static readonly int Open = Animator.StringToHash("Open");
        private Animator _animator;
        private Highlight _highlight;
        private readonly NetworkVariable<bool> _isOpened = new (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        [SerializeField] private LootTable lootTable;
        [SerializeField] private HoverInfoStats hoverInfoStats;
        [SerializeField] private Rigidbody chestLid;
        [SerializeField] private Transform spawnPoint;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _highlight =  GetComponent<Highlight>();
            _isOpened.OnValueChanged += (_, _) =>
            {
                foreach (Transform tr in transform)
                {
                    tr.gameObject.layer = LayerMask.NameToLayer("Default");
                }

                _highlight.enabled = false;
            };
        }
        

        public void OnInteract(GenericCharacter user)
        {
            if (_isOpened.Value) return;
            OpenChest_ServerRpc();
        }

        [ServerRpc(RequireOwnership = false)] //Anyone can ask the chest to open. 
        private void OpenChest_ServerRpc()
        {
            if (_isOpened.Value) return; // Additional safety check.
            
            Debug.Log("I dropped my loot");
            _animator.SetTrigger(Open);
            _highlight.enabled = false;
            _isOpened.Value = true;
        }
        
        private void ChestOpened()
        {
            if (!IsServer) return;
            chestLid.isKinematic = false;
            chestLid.AddForce(Quaternion.AngleAxis(Random.Range(-15, 15), Vector3.forward) * Quaternion.AngleAxis(Random.Range(-15, 15), Vector3.right) * Vector3.up * Random.Range(1, 25), ForceMode.Impulse);
            chestLid.AddTorque(Random.insideUnitSphere * Random.Range(1, 15), ForceMode.Impulse);
            lootTable.Spawn(spawnPoint.position, Random.Range(1, 15), Random.Range(1, 15));
        }

        public void OnHover()
        {
            _highlight.enabled = false;
        }

        public void OnHoverEnd()
        {
            _highlight.enabled = false;
        }


        public HoverInfoStats GetHoverInfoStats()
        {
            return hoverInfoStats;
        }

        public bool CanHover()
        {
            return !_isOpened.Value;
        }
    }
}
