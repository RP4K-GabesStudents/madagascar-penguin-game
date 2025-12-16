using Game.Characters.CapabilitySystem.CapabilityStats;
using Game.InventorySystem;
using Game.Objects;
using Managers;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.Penguin
{
    public class InteractionCapability : BaseCapability, IInputSubscriber
    {
        private IInteractable _interactable;
        
        private InteractionCapabilityStats _stats;

        private InventoryCapability _inventoryCapability;

        [SerializeField] private Transform interactOrigin;
        
        // Camera reference - assign in inspector or will auto-find MainCamera
        private Camera _interactionCamera;
        
        [SerializeField] private bool showDebugRays = true; // Toggle in inspector
        
        protected override void OnBound()
        {
            base.OnBound();
                                           
            _stats = genericStats as InteractionCapabilityStats;
            if (_stats == null)
            {
                Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(InteractionCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject);
            }

            _inventoryCapability = GetComponent<InventoryCapability>();
            
            // Try to find camera if not assigned
            if (_interactionCamera == null)
            {
                // Try Camera.main first
                _interactionCamera = Camera.main;
                
                // If still null, try finding any camera
                if (_interactionCamera == null)
                {
                    _interactionCamera = FindObjectOfType<Camera>();
                }
            }
            
            if (_interactionCamera == null)
            {
                Debug.LogError($"[InteractionCapability] No camera found! Please assign a camera in the inspector or tag your camera as 'MainCamera'. Object: {gameObject.name}");
            }
            else
            {
                Debug.Log($"[InteractionCapability] Using camera: {_interactionCamera.name}");
            }
        }
        
        
        public override bool CanExecute()
        {
            return _interactable != null;
        }

        protected override void Execute()
        {
            if (_interactable == null) return;
            
            Debug.Log("Interacted with:  ", (_interactable as MonoBehaviour)?.gameObject);
            
            _interactable.OnInteract(_owner);
            _owner.animator.SetTrigger(StaticUtilities.InteractAnimID);
            
            if (_interactable is Item i)
            {
                _inventoryCapability.TryPickup(i);
                Debug.Log("Successfully picked up AN ITEM " + i.name);
            }
            if(!_interactable.CanHover()) HandleHovering(null);

        }

        private void LateUpdate()
        {
            CheckForInteractable();
        }

        private void CheckForInteractable()
        {
            // Try to find camera if still null
            if (_interactionCamera == null)
            {
                _interactionCamera = Camera.main;
                if (_interactionCamera == null)
                {
                    Debug.LogWarning("Main Camera is null in CheckForInteractable!");
                    return;
                }
            }
            
            // Raycast from camera through screen center
            Ray cameraRay = _interactionCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Vector3 origin = cameraRay.origin;
            Vector3 direction = cameraRay.direction;

            if (showDebugRays)
            {
                Debug.DrawRay(origin, direction * _stats.InteractionDistance, Color.cyan, 0.1f);
            }

            bool byPass = Physics.Raycast(origin, direction, out var byPassed, _stats.InteractionDistance, _stats.CombinedLayers);
            if (byPass)
            {
                if (showDebugRays)
                {
                    Debug.DrawLine(origin, byPassed.point, Color.yellow, 0.1f);
                }
                
                bool directHit = ((1 << byPassed.collider.gameObject.layer) & _stats.InteractionLayers) != 0;
                if (directHit)
                {
                    if (showDebugRays)
                    {
                        Debug.DrawLine(origin, byPassed.point, Color.red, 0.1f);
                    }
                    
                    Rigidbody rb = byPassed.rigidbody;
                    if (rb && rb.TryGetComponent(out IInteractable interactable))
                    {
                        HandleHovering(interactable);
                        return;
                    }
                }
                else 
                {
                    bool interactHit = Physics.SphereCast(origin, _stats.InteractionRadius, direction, out RaycastHit hitInfo, _stats.InteractionDistance, _stats.InteractionLayers);
                    if (interactHit)
                    {
                        if (showDebugRays)
                        {
                            Debug.DrawLine(origin, hitInfo.point, Color.green, 0.1f);
                        }
                        
                        Rigidbody rb = hitInfo.rigidbody;
                        if (rb && rb.TryGetComponent(out IInteractable interactable))
                        {
                            HandleHovering(interactable);
                            return;
                        }
                    }
                }
            }
            HandleHovering(null);
        }


        private void OnDrawGizmos()
        {
            _stats ??= genericStats as InteractionCapabilityStats;
            if (_stats == null) return;
            
            // Get camera for gizmo visualization
            Camera cam = Camera.main;
            if (cam == null) return;
            
            // Use camera ray for gizmos (this only works in Scene view)
            Ray cameraRay = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Vector3 origin = cameraRay.origin;
            Vector3 direction = cameraRay.direction;

            // Raycast for bypass
            bool byPass = Physics.Raycast(origin, direction, out RaycastHit byPassed, _stats.InteractionDistance, _stats.CombinedLayers);
            if (byPass)
            {
                // Draw bypass ray
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(origin, byPassed.point);
                Gizmos.DrawWireSphere(byPassed.point, 0.05f);

                // Check for direct hit
                bool directHit = ((1 << byPassed.collider.gameObject.layer) & _stats.InteractionLayers) != 0;
                if (directHit)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(byPassed.point, 0.15f); // Indicate direct hit
                    return;
                }
                // SphereCast fallback
                bool interactHit = Physics.SphereCast(origin, _stats.InteractionRadius, direction, out RaycastHit hitInfo, _stats.InteractionDistance, _stats.InteractionLayers);
                if (interactHit)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(origin, hitInfo.point);
                    Gizmos.DrawWireSphere(hitInfo.point, _stats.InteractionRadius);
                    return;
                }

                // Draw max reach for failed spherecast
                Vector3 end = origin + direction * _stats.InteractionDistance;
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                Gizmos.DrawWireSphere(end, _stats.InteractionRadius);
                Gizmos.DrawLine(origin, end);
            }
            else
            {
                // Nothing hit: show max reach ray
                Vector3 end = origin + direction * _stats.InteractionDistance;
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(origin, end);
            }
        }

        private void HandleHovering(IInteractable interactable)
        {
            if (_interactable == interactable) return;
            if ((Behaviour)_interactable) _interactable.OnHoverEndDriver();
            _interactable?.OnHoverEndDriver();
            if(interactable != null && interactable.CanHover()) interactable.OnHoverDriver();
            _interactable = interactable;
        }

        public void BindControls(GameControls controls)
        {
            controls.Player.Interact.performed += _ => TryExecute();
        }
    }
}