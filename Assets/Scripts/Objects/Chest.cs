using Interfaces;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using ResourceManager = Managers.ResourceManager;

namespace Objects
{
    public class Chest : NetworkBehaviour, IInteractable
    {
        private static readonly int Open = Animator.StringToHash("Open");
        [SerializeField] private LootTable lootTable;
        [SerializeField] private HoverInfoStats hoverInfoStats;
        private Animator _animator;
        [SerializeField] private MeshRenderer[] meshRenderers;
        private readonly NetworkVariable<bool> _isOpened = new (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private Rigidbody chestLid;
        [SerializeField] private Transform spawnPoint;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _isOpened.OnValueChanged += (_, _) => OnHoverEnd();
        }
        
        public void OnHover()
        {
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.sharedMaterials = new[] { meshRenderer.sharedMaterials[0], ResourceManager.Instance.HoverMaterial };
            }
        }

        public void OnHoverEnd()
        {
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.sharedMaterials = new[] { meshRenderer.sharedMaterials[0] };
            }
        }

        public void OnInteract()
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
            OnHoverEnd();
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
