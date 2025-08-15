using System;
using System.Collections.Generic;
using Game.Characters.Stats;
using Interfaces;
using Inventory;
using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// A Generic Character is like a husk; it describes how a character might work, but not what who controls it, or how it is controlled.
    /// The Generic Character should not query outside scripts
    /// </summary>
    [SelectionBase, RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class GenericCharacter : NetworkBehaviour, IDamageable, IInputSubscriber
    {
        
        [SerializeField] private Transform head;
        [SerializeField] protected CharacterStats stats;
        
        //This dictionary contains state information
        private readonly Dictionary<uint, int> _data = new();
        
        protected float _currentResistance = 0;
        private float _curHealth;

        public Rigidbody rigidbody { get; private set; }
        public Animator animator { get; private set; }
        public AnInventory inventory { get; private set; }
        
        public Transform Head => head;

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

                if (_curHealth <= 0) Die(Vector3.zero);
            }
        }

        public float DamageRes => Mathf.Min(0.9f, stats.BaseResistance + _currentResistance);
        public float HealthPercent => Health / stats.Hp;
        
        public int GetDataDictionaryValue(uint key) => _data[key];

        //We want this as a function so we can bind information to keys.
        public void SetDataDictionaryValue(uint key, int value) => _data[key] = value;
        public bool TryAddDataKey(uint key, int initialValue) => _data.TryAdd(key, initialValue);

        #region Actions

        //-------------- Actions -----------//
        //Austin, add more if needed... These are observables used in Effects, Abilities, AI and UI
        //When might we need to indicate to one of those subsystem that something has happened?
        //For instance; if the AI takes damage (OnHealthUpdated) maybe it gets scared and runs away.
        public event Action<float> OnHealthUpdated;
        public event Action OnDeath;
        public event Action OnAttack;


        #endregion

        private void Awake()
        {
            InitializeComponents();
            Debug.LogWarning("It'd be nice, if when we created, we read a list of stats objects, and generated our ability scripts to go with that.");
            // InitializeAbilities();
        }

        protected virtual void InitializeComponents()
        {
            rigidbody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            inventory = GetComponent<AnInventory>();
        }
        
        #region Damagable
        public virtual void OnHurt(float amount, Vector3 force) 
        {
            OnHealthUpdated?.Invoke(amount);
            rigidbody.AddForce(force, ForceMode.Impulse);
            Debug.LogWarning($"{name} was hit for {amount}, Play on hit animation",gameObject);
        }

        public virtual void Die(Vector3 force)
        {
            Debug.Log($"{name} Died",gameObject);
            OnDeath?.Invoke();
        }
        #endregion
        
        public void BindControls(GameControls controls)
        {
            controls.Player.Attack.performed += ctx => animator.SetBool(StaticUtilities.AttackAnimID, ctx.ReadValueAsButton());
        }
        
        public void ExecuteAttack()
        {
            Debug.Log("Trying to attack");
            OnAttack?.Invoke();
        }
    }
}