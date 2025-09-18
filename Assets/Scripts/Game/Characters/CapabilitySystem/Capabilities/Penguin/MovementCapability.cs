using Game.Characters.CapabilitySystem.CapabilityStats.Penguin;
using Managers;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.Penguin
{
    public class MovementCapability : BaseCapability, IInputSubscriber
    {
        private Vector3 _curMoveDir;
        private MovementCapabilityStats _stats;
        private Rigidbody _rigidbody;
        private Animator _animator;
        
        private bool _isGrounded;
        private Vector3 _groundCheckPosition;

        public override bool CanExecute() => true;
        protected override void Execute() { }

        protected override void OnBound()
        {
            base.OnBound();
            _stats = genericStats as MovementCapabilityStats;
            if (_stats == null) { Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(MovementCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject); }

            _owner.TryAddDataKey(CapabilityKeys.CurrentVelocityXZ, 0);
            _owner.TryAddDataKey(CapabilityKeys.CurrentVelocityY, 0);

            _rigidbody = _owner.rigidbody;
            _animator = _owner.GetComponent<Animator>();
        }

        public void BindControls(GameControls controls)
        {
            controls.Player.Move.performed += ctx => SetMoveDirection(ctx.ReadValue<Vector2>());
        }
        public void SetMoveDirection(Vector2 moveDirection)
        {
            _curMoveDir = new Vector3(moveDirection.x, 0, moveDirection.y);
        }
        
        private void FixedUpdate()
        {
            HandleMoving();
            HandleGrounding();
        }

        private void HandleMoving()
        {
            _rigidbody.AddForce(transform.rotation * _curMoveDir * _stats.Speed);
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            Vector2 xz = new Vector2(currentVelocity.x, currentVelocity.z);
            float magnitudeXZ = xz.magnitude;

            if (magnitudeXZ > _stats.MaxSpeed)
            {
                Vector2 dir = xz / magnitudeXZ;
                magnitudeXZ = _stats.MaxSpeed;
                _rigidbody.linearVelocity = new Vector3(dir.x, currentVelocity.y, dir.y);
            }
            
            _owner.SetDataDictionaryValue(CapabilityKeys.CurrentVelocityXZ, magnitudeXZ.FloatAsInt());
            _owner.SetDataDictionaryValue(CapabilityKeys.CurrentVelocityY, currentVelocity.y.FloatAsInt());
            
            _animator.SetFloat(StaticUtilities.ForwardAnimID, magnitudeXZ);
        }

        private void HandleGrounding()
        {
            // Calculate the ground check position
            _groundCheckPosition = transform.position + _stats.GroundCheckOffset;
            
            // Perform the ground check using SphereCast
            bool wasGrounded = _isGrounded;
            _isGrounded = Physics.SphereCast(
                _groundCheckPosition, 
                _stats.GroundCheckRadius, 
                Vector3.down, 
                out RaycastHit hit,
                _stats.GroundCheckDistance, 
                StaticUtilities.GroundLayers
            );
            
            // Update the data dictionary with grounded state
            _owner.SetDataDictionaryValue(CapabilityKeys.IsGrounded, _isGrounded.BoolAsInt());
            
            /*
            // Optional: Log grounding state changes for debugging
            if (wasGrounded != _isGrounded)
            {
                Debug.Log($"Grounding state changed: {_isGrounded} on object: {hit.collider?.name ?? "None"}");
            }
            */
        }

        private void OnDrawGizmos()
        {
            if (_stats == null) return;

            // Calculate gizmo position
            Vector3 gizmoPos = transform.position + _stats.GroundCheckOffset;
            
            // Set gizmo color based on grounded state
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            
            // Draw the sphere at the starting position
            Gizmos.DrawWireSphere(gizmoPos, _stats.GroundCheckRadius);
            
            // Draw the raycast line
            Vector3 endPos = gizmoPos + Vector3.down * _stats.GroundCheckDistance;
            Gizmos.DrawLine(gizmoPos, endPos);
            
            // Draw the sphere at the end position to show the full detection area
            Gizmos.color = _isGrounded ? Color.green * 0.5f : Color.red * 0.5f;
            Gizmos.DrawWireSphere(endPos, _stats.GroundCheckRadius);
            
            // Add a label for debugging
            UnityEditor.Handles.Label(gizmoPos + Vector3.up * 0.5f, $"Grounded: {_isGrounded}");
        }
        
    }
}