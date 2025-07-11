using System;
using Abilities;
using Interfaces;
using Inventory;
using Items;
using Items.Weapons;
using Managers;
using Scriptable_Objects;
using Scriptable_Objects.Penguin_Stats;
using UnityEngine;

namespace penguin
{
    
    public class PlayerController : BasePenguinFile, IDamageable
    {
        private Player _controlled;
        [SerializeField] PenguinStats penguinStats;
        public PenguinStats PenguinoStats => penguinStats;
        [SerializeField] private ProjectileStats projectileStats;
        [SerializeField] private MorePenguinStats morePenguinStats;
        private Rigidbody _rigidbody;
        private Vector3 _curMoveDir;
        private Vector2 _curLookDir;
        private float _curHealth;
        private IInteractable _interactable;
        private Item _curItem;
        
        [SerializeField] public Transform laserSpawn;
        [SerializeField] public Transform laserSpawn2;
        [SerializeField] private Transform attackLocation;
        public event Action<float> onHealthUpdated;
        public event Action onAttack;
        public event Action onDeath;
        [Header("Jumping")] 
    
    
        [Header("Animation")]
        [SerializeField] private Transform headProxy;
        [SerializeField] private Transform headBone;
        [SerializeField, Min(0)] private float rotationAnimationSpeed;
        [SerializeField, Min(0)] private float animationReturnSpeed;

        [SerializeField, Range(-1,1)] private float rotationAnimationThreshold;

    
        [Header("Looking")] 
        [SerializeField]
        public Transform headXRotator;
        [SerializeField] private Transform bodyYRotator;
        [SerializeField, Min(0)] private float rotationSpeed;
        [SerializeField, Range(0,90)] private float pitchLimit;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void BindController(Player player)
        {
            _controlled = player;
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.rotation * _curMoveDir * penguinStats.Speed);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            HandleLooking();
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackLocation.position, penguinStats.AttackRadius);
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
            headProxy.Rotate(Vector3.up, _curLookDir.x * rotationAnimationSpeed * dt);
            headProxy.Rotate(Vector3.right, _curLookDir.y * rotationAnimationSpeed * dt);

            //Check if we exceed the threshold, if we do then rotate the head slowly to correct location
            if (Vector3.Dot(headProxy.forward, headXRotator.forward) < rotationAnimationThreshold)
            {
                headProxy.rotation= Quaternion.Slerp(headProxy.rotation, headXRotator.rotation, animationReturnSpeed * dt); // Use rotation speed
            }
       
            headBone.rotation = headProxy.rotation;
        
        }
        private void Update()
        {
            if (!IsOwner) return;
            if (penguinStats.Speed >= penguinStats.SpeedLimit) penguinStats.Speed = penguinStats.SpeedLimit;
            CheckForInteractable();
        }
        public void Jump(bool readValueAsButton)
        {
            if (!_ikController.IsGrounded) return;
            _rigidbody.AddForce(transform.up * penguinStats.JumpPower, ForceMode.Impulse);
            _animator.SetTrigger(StaticUtilities.JumpingAnimID);
        }
        public void Attack(bool readValueAsButton)
        {
            if (!readValueAsButton) return;
            _animator.SetTrigger(StaticUtilities.AttackAnimID);
        }

        public void ExecuteAttack()
        {
            //This animation should actually probably be controlled by the weapon at this point.
            _curItem?.UseItem();
            onAttack?.Invoke();
        }
        
        public void Sprint(bool readValueAsButton)
        {
        
        }
        public void Crouch(bool readValueAsButton)
        {
            _animator.SetTrigger(StaticUtilities.SlidingAnimID);
        }
        public void Interact(bool readValueAsButton)
        {
            if (_interactable == null || !readValueAsButton) return;
            if (_interactable is Item i)
            {
                if (_controlled.HeyIPickedSomethingUp(i.ItemStats))
                {
                    _animator.SetTrigger(StaticUtilities.InteractAnimID);
                    Debug.LogWarning("success");
                    _interactable.OnInteract(this);
                    if(!_interactable.CanHover()) HandleHovering(null);
                }
                else
                {
                    Debug.LogWarning("do a barrel roll");
                }
            }
            else
            {
                _animator.SetTrigger(StaticUtilities.InteractAnimID);
                _interactable.OnInteract(this);
                if(!_interactable.CanHover()) HandleHovering(null);
            }
        }
        
        

        private void CheckForInteractable()
        {
            
            bool interactHit = Physics.SphereCast(headXRotator.position, morePenguinStats.InteractRadius, headXRotator.forward, out RaycastHit hitInfo, morePenguinStats.InteractDistance, morePenguinStats.InteractLayer);
            if (interactHit)
            {
                bool hitWall = Physics.Raycast(headXRotator.position, headXRotator.forward, out _, morePenguinStats.InteractRadius, StaticUtilities.GroundLayers);
                if (hitWall)
                {
                    HandleHovering(null);
                    return;
                }
                
                Debug.DrawLine(headXRotator.position, hitInfo.point, Color.green, 0.1f);
                Rigidbody rb = hitInfo.rigidbody;
                if (rb && rb.TryGetComponent(out IInteractable interactable))
                {
                    HandleHovering(interactable);
                    return;
                }
            }
            HandleHovering(null);
        }

        private void HandleHovering(IInteractable interactable)
        {
            if (_interactable == interactable) return;
            if ((Behaviour)_interactable != null) _interactable.OnHoverEndDriver();
            _interactable?.OnHoverEndDriver();
            if(interactable != null && interactable.CanHover()) interactable.OnHoverDriver();
            _interactable = interactable;
        }

        public void SetMoveDirection(Vector3 moveDirection)
        {
            _curMoveDir = moveDirection;
        }
        public void Look(Vector2 lookDirection)
        {
            _curLookDir = lookDirection;
        }
        public override float GetHorizontalSpeed()
        {
            return (new Vector2(_rigidbody.linearVelocity.x, _rigidbody.linearVelocity.z)).magnitude;
        }

        public void Die()
        {
            Debug.Log("you died");
        }

        public void OnHurt(float amount, Vector3 force)
        {
            Debug.Log("you got hit for: " + amount);
            _rigidbody.AddForce(force, ForceMode.Impulse);
            Health = penguinStats.Hp - amount;
        }
        

        public float Health
        {
            get => _curHealth;
            set
            {
               float t = Mathf.Min(_curHealth + value, penguinStats.Hp);
               if (!Mathf.Approximately(t, _curHealth))
               {
                   float diff = t - _curHealth;
                   _curHealth = t;
                   onHealthUpdated?.Invoke(diff);
               }
            }
            
        }

        public float DamageRes { get => 0; }

    }
}
