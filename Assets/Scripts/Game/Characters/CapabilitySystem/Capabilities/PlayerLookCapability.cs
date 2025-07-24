using Game.Characters.CapabilitySystem.Capabilities;
using UnityEngine;

namespace Game.Characters.Movement
{
    public class PlayerLookCapability : BaseCapability, IInputSubscriber 
    {
        [SerializeField] private Transform headXRotator;
        [SerializeField] private Transform bodyYRotator;
        
        [Header("Looking")] 
        [SerializeField, Min(0)] private float rotationSpeed;
        [SerializeField, Range(0,90)] private float pitchLimit;
        
        [Header("Animation")]
        [SerializeField, Min(0)] private float rotationAnimationSpeed;
        [SerializeField, Min(0)] private float animationReturnSpeed;
        [SerializeField, Range(-1,1)] private float rotationAnimationThreshold;

        private Vector2 _curLookDir;
        
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
            bodyYRotator.Rotate(Vector3.up, _curLookDir.x * rotationSpeed * dt);
        
            //Next, we want to rotate around the X axis, BUT we need to be careful to not look to high up.
            float newXRotation = headXRotator.localEulerAngles.x;
        
            newXRotation = (newXRotation > 180f) ? newXRotation - 360f : newXRotation; // Convert to -180 to 180 range (Because Unity sucks)
        
            newXRotation = Mathf.Clamp(newXRotation + _curLookDir.y * rotationSpeed * dt, -pitchLimit, pitchLimit);
        
            //Set the rotation
            headXRotator.localRotation = Quaternion.Euler(newXRotation, headXRotator.localEulerAngles.y, headXRotator.localEulerAngles.z);
        
            //Rotate the head in both directions...
            owner.Head.Rotate(Vector3.up, _curLookDir.x * rotationAnimationSpeed * dt);
            owner.Head.Rotate(Vector3.right, _curLookDir.y * rotationAnimationSpeed * dt);

            //Check if we exceed the threshold, if we do then rotate the head slowly to correct location
            if (Vector3.Dot(owner.Head.forward, headXRotator.forward) < rotationAnimationThreshold)
            {
                owner.Head.rotation= Quaternion.Slerp(owner.Head.rotation, headXRotator.rotation, animationReturnSpeed * dt); // Use rotation speed
            }
       
            owner.Head.rotation = owner.Head.rotation;
        }
    }
}