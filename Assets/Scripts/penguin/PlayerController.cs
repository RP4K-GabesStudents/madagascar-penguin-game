using System.Collections;
using Interfaces;
using Managers;
using Objects;
using Scriptable_Objects;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace penguin
{
    
    public class PlayerController : BasePenguinFile, IDamageable
    {
        [SerializeField]PenguinStats penguinStats;
        private ProjectileStats _projectileStats;
        private Rigidbody _rigidbody;
        private Vector3 _curMoveDir;
        private Vector2 _curLookDir;
        private float _curHealth;
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] private Transform laserSpawn;
        [SerializeField] private Transform laserSpawn2;
        [SerializeField]private Transform attackLocation;
        
        [Header("Jumping")] 
    
    
        [Header("Animation")]
        [SerializeField] private Transform headProxy;
        [SerializeField] private Transform headBone;
        [SerializeField, Min(0)] private float rotationAnimationSpeed;
        [SerializeField, Min(0)] private float animationReturnSpeed;

        [SerializeField, Range(-1,1)] private float rotationAnimationThreshold;

    
        [Header("Looking")] 
        [SerializeField] private Transform headXRotator;
        [SerializeField] private Transform bodyYRotator;
        [SerializeField, Min(0)] private float rotationSpeed;
        [SerializeField, Range(0,90)] private float pitchLimit;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
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
            if (penguinStats.Speed >= penguinStats.SpeedLimit) penguinStats.Speed = penguinStats.SpeedLimit;
        }
        public void Jump(bool readValueAsButton)
        {
            if (!_ikController.IsGrounded) return;
            _rigidbody.AddForce(transform.up * penguinStats.JumpPower, ForceMode.Impulse);
            _animator.SetTrigger(StaticUtilities.JumpingAnimID);
        }
        public void Attack(bool readValueAsButton)
        {
            _animator.SetBool(StaticUtilities.AttackAnimID, readValueAsButton);
        }

        public void ExecuteAttack()
        {
            if (penguinStats.CanShootLaser) StartCoroutine(LaserShoot());
            bool success = Physics.SphereCast(attackLocation.position, penguinStats.AttackRadius, attackLocation.forward, out RaycastHit hitInfo, penguinStats.MaxAttackDist, StaticUtilities.AttackableLayers);
            Debug.DrawRay(attackLocation.position, attackLocation.forward * penguinStats.MaxAttackDist, Color.magenta, 3f);
            if (!success) return;
            Rigidbody hitInfoRigidbody = hitInfo.rigidbody;
            Debug.DrawLine(attackLocation.position, hitInfo.point, Color.green, 3f);
            if (hitInfoRigidbody && hitInfoRigidbody.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(penguinStats.Damage, attackLocation.forward * penguinStats.KnockbackPower);
            }
        }

        IEnumerator LaserShoot()
        {
            if (!penguinStats.CanShootLaser) yield break;

            Instantiate(projectilePrefab, laserSpawn.position, transform.rotation);
            Instantiate(projectilePrefab, laserSpawn2.position, transform.rotation);
            yield return new WaitForSeconds(_projectileStats.LaserAbilityTime);
            penguinStats.CanShootLaser = false;
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
            _animator.SetTrigger(StaticUtilities.InteractAnimID);
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
            Debug.Log("you hit " + amount);
            _rigidbody.AddForce(force, ForceMode.Impulse);
        }
        

        public float Health { get => _curHealth; set => _curHealth = Mathf.Min(_curHealth + value, penguinStats.Hp); }
        public float DamageRes { get => 0; }
    }
}
