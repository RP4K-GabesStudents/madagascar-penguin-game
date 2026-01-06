using UnityEngine;

namespace Detection.Controllers
{
    public class DetectionStatusToMesh : MonoBehaviour
    {
        private static readonly int Fill = Shader.PropertyToID("_Fill");
        
        [SerializeField] private DetectionController detectionController;
        [SerializeField] private MeshRenderer meshRenderer;
        
        private Material _detectionMaterial;
        
        private void Awake()
        {
            if (meshRenderer != null)
            {
                _detectionMaterial = meshRenderer.material;
                _detectionMaterial.SetFloat(Fill, 0);
            }
        }
        
        private void Update()
        {
            if (_detectionMaterial == null || detectionController == null) return;
            
            float detectionPercent = detectionController.DetectionPercent;
            _detectionMaterial.SetFloat(Fill, detectionPercent);
        }
        
        private void OnDestroy()
        {
            // Clean up the material instance to avoid memory leaks
            if (_detectionMaterial != null)
            {
                Destroy(_detectionMaterial);
            }
        }
    }
}