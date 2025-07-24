using Interfaces;
using Managers;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    public class InteractionCapability : BaseCapability
    {
        private IInteractable _interactable;

        
        public override bool CanExecute()
        {
            throw new System.NotImplementedException();
        }

        protected override void Execute()
        {
            throw new System.NotImplementedException();
        }

        private void LateUpdate()
        {
            CheckForInteractable();
        }

        private void CheckForInteractable()
        {

            Vector3 origin = owner.Head.position;
            Vector3 direction = owner.Head.forward;
            
            bool interactHit = Physics.SphereCast(origin, morePenguinStats.InteractRadius, direction, out RaycastHit hitInfo, morePenguinStats.InteractDistance, morePenguinStats.InteractLayer);
            if (interactHit)
            {
                bool hitWall = Physics.Raycast(origin, direction, out _, morePenguinStats.InteractRadius, StaticUtilities.GroundLayers);
                if (hitWall)
                {
                    HandleHovering(null);
                    return;
                }
                
                Debug.DrawLine(origin, hitInfo.point, Color.green, 0.1f);
                Rigidbody rb = hitInfo.rigidbody;
                if (rb && rb.TryGetComponent(out IInteractable interactable))
                {
                    HandleHovering(interactable);
                    return;
                }
            }
            HandleHovering(null);
        }

        private void OnDrawGizmosSelected()
        {
            
        }

        private void HandleHovering(IInteractable interactable)
        {
            if (_interactable == interactable) return;
            if ((Behaviour)_interactable != null) _interactable.OnHoverEndDriver();
            _interactable?.OnHoverEndDriver();
            if(interactable != null && interactable.CanHover()) interactable.OnHoverDriver();
            _interactable = interactable;
        }
    }
}