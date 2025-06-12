using Interfaces;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.Serialization;
using ResourceManager = Managers.ResourceManager;

namespace Objects
{
    public class Chest : MonoBehaviour, IInteractable
    {
        [SerializeField] private LootTable lootTable;
        [SerializeField] private HoverInfoStats hoverInfoStats;
        private MeshRenderer[] _meshRenderers;
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

        public void OnInteract()
        {
            Debug.Log("I dropped my loot");
            StartCoroutine(lootTable.Spawn(transform.position, 3, 0.1f));
        }

        public HoverInfoStats GetHoverInfoStats()
        {
            return hoverInfoStats;
        }
    }
}
