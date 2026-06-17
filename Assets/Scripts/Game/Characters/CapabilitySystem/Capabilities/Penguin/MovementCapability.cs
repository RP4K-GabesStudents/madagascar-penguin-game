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

        private InventoryCapability _inventory; // for equipped-weapon modifiers

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
            _owner.TryAddDataKey(CapabilityKeys.IsGrounded, 1);

            _rigidbody = _owner.rigidbody;
            _animator = _owner.GetComponent<Animator>();
            _inventory = GetComponent<InventoryCapability>();
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
            // Equipped weapon (if any) scales speed and top speed.
            float speedMul = 1f, maxSpeedMul = 1f;
            var weapon = _inventory ? _inventory.EquippedWeapon : null;
            if (weapon && weapon.Stats)
            {
                speedMul = weapon.Stats.MoveSpeedMultiplier;
                maxSpeedMul = weapon.Stats.MaxSpeedMultiplier;
            }

            _rigidbody.AddForce(transform.rotation * _curMoveDir * (_stats.Speed * speedMul));
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            Vector2 xz = new Vector2(currentVelocity.x, currentVelocity.z);
            float magnitudeXZ = xz.magnitude;

            float maxSpeed = _stats.MaxSpeed * maxSpeedMul;
            if (magnitudeXZ > maxSpeed)
            {
                Vector2 dir = xz / magnitudeXZ;
                dir *= maxSpeed;
                _rigidbody.linearVelocity = new Vector3(dir.x, currentVelocity.y, dir.y);
                magnitudeXZ = maxSpeed;
            }

            _owner.SetDataDictionaryValue(CapabilityKeys.CurrentVelocityXZ, magnitudeXZ.FloatAsInt());
            _owner.SetDataDictionaryValue(CapabilityKeys.CurrentVelocityY, currentVelocity.y.FloatAsInt());

            _animator.SetFloat(StaticUtilities.ForwardAnimID, magnitudeXZ);
        }

        private void HandleGrounding()
        {
            _groundCheckPosition = transform.position + _stats.GroundCheckOffset;

            bool wasGrounded = _isGrounded;
            _isGrounded = Physics.SphereCast(
                _groundCheckPosition,
                _stats.GroundCheckRadius,
                Vector3.down,
                out RaycastHit hit,
                _stats.GroundCheckDistance,
                StaticUtilities.GroundLayers
            );

            _owner.SetDataDictionaryValue(CapabilityKeys.IsGrounded, _isGrounded.BoolAsInt());
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_stats == null) return;

            Vector3 gizmoPos = transform.position + _stats.GroundCheckOffset;

            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(gizmoPos, _stats.GroundCheckRadius);

            Vector3 endPos = gizmoPos + Vector3.down * _stats.GroundCheckDistance;
            Gizmos.DrawLine(gizmoPos, endPos);

            Gizmos.color = _isGrounded ? Color.green * 0.5f : Color.red * 0.5f;
            Gizmos.DrawWireSphere(endPos, _stats.GroundCheckRadius);

            UnityEditor.Handles.Label(gizmoPos + Vector3.up * 0.5f, $"Grounded: {_isGrounded}");
        }
#endif
    }
}