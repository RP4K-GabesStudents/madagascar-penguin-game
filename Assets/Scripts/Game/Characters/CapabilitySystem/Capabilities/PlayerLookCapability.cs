using Game.Characters.CapabilitySystem.Capabilities;
using Game.Characters.CapabilitySystem.CapabilityStats;
using UnityEngine;

namespace Game.Characters.Movement
{
    public class PlayerLookCapability : BaseCapability, IInputSubscriber 
    {
        [SerializeField] private Transform headXRotator;
        [SerializeField] private Transform bodyYRotator;

        private LookCapabilityStats _stats;
        private Vector2 _curLookDir;


        protected override void OnBound()
        {
            base.OnBound();
            
            _stats = genericStats as LookCapabilityStats;
            if (_stats == null) { Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(LookCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject); }

        }

        public override bool CanExecute() { return true; }
        protected override void Execute() { }

        public void BindControls(GameControls controls)
        {
            controls.Player.Look.performed +=  ctx => Look(ctx.ReadValue<Vector2>());
        }
        
        public void Look(Vector2 lookDirection)
        {
            _curLookDir = lookDirection;
        }
        
        private void LateUpdate()
        {
            HandleLooking();
        }

        

        private void HandleLooking()
        {
            float dt = Time.deltaTime; //Cache this for easy optimization
        
            //First, let's rotate around the Y axis (left and right rotation)
            bodyYRotator.Rotate(Vector3.up, _curLookDir.x * _stats.RotationSpeed * dt);
        
            //Next, we want to rotate around the X axis, BUT we need to be careful to not look to high up.
            float newXRotation = headXRotator.localEulerAngles.x;
        
            newXRotation = (newXRotation > 180f) ? newXRotation - 360f : newXRotation; // Convert to -180 to 180 range (Because Unity sucks)
        
            newXRotation = Mathf.Clamp(newXRotation + _curLookDir.y * _stats.RotationSpeed * dt, -_stats.PitchLimit, _stats.PitchLimit);
        
            //Set the rotation
            headXRotator.localRotation = Quaternion.Euler(newXRotation, headXRotator.localEulerAngles.y, headXRotator.localEulerAngles.z);
        
            //Rotate the head in both directions...
            owner.Head.Rotate(Vector3.up, _curLookDir.x * _stats.RotationAnimationSpeed * dt);
            owner.Head.Rotate(Vector3.right, _curLookDir.y * _stats.RotationAnimationSpeed * dt);

            //Check if we exceed the threshold, if we do then rotate the head slowly to correct location
            if (Vector3.Dot(owner.Head.forward, headXRotator.forward) < _stats.RotationAnimationThreshold)
            {
                owner.Head.rotation= Quaternion.Slerp(owner.Head.rotation, headXRotator.rotation, _stats.AnimationReturnSpeed * dt); // Use rotation speed
            }
       
            owner.Head.rotation = owner.Head.rotation;
        }
    }
}