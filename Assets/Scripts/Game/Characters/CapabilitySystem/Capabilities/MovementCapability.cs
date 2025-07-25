using Game.Characters.CapabilitySystem.CapabilityStats;
using Game.Characters.Movement;
using Managers;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities
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

            owner.TryAddDataKey(CapabilityKeys.CurrentVelocityXZ, 0);
            owner.TryAddDataKey(CapabilityKeys.CurrentVelocityY, 0);

            _rigidbody = owner.rigidbody;
            _animator = owner.GetComponent<Animator>();
        }

        public void BindControls(GameControls controls)
        {
            controls.Player.Move.performed += ctx => SetMoveDirection(ctx.ReadValue<Vector2>());
        }
        public void SetMoveDirection(Vector3 moveDirection)
        {
            _curMoveDir = moveDirection;
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
            
            owner.SetDataDictionaryValue(CapabilityKeys.CurrentVelocityXZ, magnitudeXZ.FloatAsInt());
            owner.SetDataDictionaryValue(CapabilityKeys.CurrentVelocityY,  currentVelocity.y.FloatAsInt());
            
            _animator.SetFloat(StaticUtilities.ForwardAnimID, magnitudeXZ);
        }

        private void HandleGrounding()
        {
            //Implement later
            owner.SetDataDictionaryValue(CapabilityKeys.IsGrounded, true.BoolAsInt());
        }
        
    }
}