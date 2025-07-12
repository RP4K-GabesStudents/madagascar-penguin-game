using System;
using AbilitySystem.Abilities;
using Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// A Generic Character is like a husk; it describes how a character might work, but not what who controls it, or how it is controlled.
    /// The Generic Character should not query outside scripts
    /// </summary>
    [SelectionBase,RequireComponent(typeof(AudioSource), typeof(Rigidbody))]
    public class GenericCharacter : NetworkBehaviour, IDamageable
    {

        #region Transform
        //---------- Transform Information -----------//
        //These objects are useful for retrieving important common data from our characters
        [Header("Transforms")]
        [SerializeField] private Transform[] eyes;
        [SerializeField] private Transform head;
        
        [SerializeField] private Transform headXRotator;
        [SerializeField] private Transform bodyYRotator;

        public Transform[] Eyes => eyes;
        public Transform Head => head;
        
        #endregion

        #region Character Info
        //---------- Character Information -----------//
        //Here is information that is necessary for all character object creation.
        
        [Header("Stats")] 
        [SerializeField] protected CharacterStats stats;
        protected float _currentResistance = 0;
        private float _curHealth;
        
        
        
        public float Health
        {
            get => _curHealth;
            set
            {
                float t = Mathf.Min(_curHealth + value, stats.Hp);
                if (!Mathf.Approximately(t, _curHealth))
                {
                    float diff = t - _curHealth;
                    _curHealth = t;
                    OnHealthUpdated?.Invoke(diff);
                }
            }
        }
        public float DamageRes => Mathf.Min(0.9f, stats.BaseResistance + _currentResistance);
        public float HealthPercent => Health / stats.Hp;

        #endregion

        #region Components
      
        //---------- Components -----------//
        protected Animator animator;
        
        // NOTE: we say new, because unity is dumb and used to force you to have a rigidbody on every object... The legacy code still exists, even though it doesn't work.
        protected new Rigidbody rigidbody; 
        
        #endregion
        
        #region Actions

        //-------------- Actions -----------//
        //Austin, add more if needed... These are observables used in Effects, Abilities, AI and UI
        //When might we need to indicate to one of those subsystem that something has happened?
        //For instance; if the AI takes damage (OnHealthUpdated) maybe it gets scared and runs away.
        public event Action<float> OnHealthUpdated;
        public event Action OnAttack;
        public event Action OnDeath;
        
        #endregion

        private void Awake()
        {
            InitializeComponents();
            InitializeAbilities();
        }

        protected virtual void InitializeComponents()
        {
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void InitializeAbilities()
        {
            foreach (GenericAbility ability in stats.DefaultAbilities)
            {
                GenericAbility spawned = Instantiate(ability, transform);
                spawned.NetworkObject.SpawnWithOwnership(OwnerClientId);
            }
        }
        #region Movement
        
        private void HandleGround()
        {
            
        }
        protected virtual void OnGrounded()
        {

        }
        
        protected virtual void OnUngrounded()
        {
        }
        
        #endregion
        
        
        #region Damagable
        public virtual void OnHurt(float amount, Vector3 force) 
        {
            rigidbody.AddForce(force, ForceMode.Impulse);
            Debug.Log($"{name} was hit for {amount}",gameObject);
        }

        public virtual void Die(Vector3 force)
        {
            Debug.Log($"{name} Died",gameObject);
        }
        #endregion
    }
}