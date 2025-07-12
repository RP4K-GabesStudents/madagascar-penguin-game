using System.Collections;
using Managers;
using UnityEngine;

namespace Game.Objects
{
    public class Highlight : MonoBehaviour
    {
        
        [SerializeField] private MeshRenderer[] meshRenderers;

        
        private void Awake()
        {
            RecompileMesh();
        }

        private void OnEnable()
        {
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.sharedMaterials = new[] { meshRenderer.sharedMaterials[0], ResourceManager.Instance.HoverMaterial };
            }
        }

        private void OnDisable()
        {
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.sharedMaterials = new[] { meshRenderer.sharedMaterials[0] };
            }
        }

        [ContextMenu("RecompileMeshRendererList")]
        public void RecompileMesh()
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }
        
        #if UNITY_EDITOR
        private Coroutine _rot;
        private void OnDrawGizmosSelected()
        {
            if (meshRenderers.Length == 0 && _rot == null) _rot = StartCoroutine(EditorTryAutoPop());
        }

        private IEnumerator EditorTryAutoPop()
        {
            while (meshRenderers.Length == 0)
            {
                RecompileMesh();
                if (meshRenderers.Length == 0)
                {
                    Debug.LogWarning("There are no mesh renderers attached to this object, but we're trying to highlight it?", gameObject);
                    yield return new WaitForSeconds(10);
                }
            }

            _rot = null;
        }
        
        
        #endif
    }
}