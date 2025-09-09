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
            Vector3 currentVelocity =  _rigidbody.linearVelocity;
            Vector2 xz = new Vector2(currentVelocity.x, currentVelocity.z);
            float magnitudeXZ = xz.magnitude;

            if (magnitudeXZ > _stats.MaxSpeed)
            {
                Vector2 dir  = xz /  magnitudeXZ;
                magnitudeXZ = _stats.MaxSpeed;
                _rigidbody.linearVelocity = new Vector3(dir.x, currentVelocity.y, dir.y);
            }
            
            _owner.SetDataDictionaryValue(CapabilityKeys.CurrentVelocityXZ, magnitudeXZ.FloatAsInt());
            _owner.SetDataDictionaryValue(CapabilityKeys.CurrentVelocityY,  currentVelocity.y.FloatAsInt());
            
            _animator.SetFloat(StaticUtilities.ForwardAnimID, magnitudeXZ);
        }

        private void HandleGrounding()
        {
            //Implement later
            _owner.SetDataDictionaryValue(CapabilityKeys.IsGrounded, true.BoolAsInt());
        }
        
    }
}