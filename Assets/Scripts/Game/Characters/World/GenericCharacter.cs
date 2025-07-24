using System;
using System.Collections.Generic;
using Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// A Generic Character is like a husk; it describes how a character might work, but not what who controls it, or how it is controlled.
    /// The Generic Character should not query outside scripts
    /// </summary>
    [SelectionBase, RequireComponent(typeof(Rigidbody))]
    public class GenericCharacter : NetworkBehaviour, IDamageable
    {

        #region Transform
        //---------- Transform Information -----------//
        //These objects are useful for retrieving important common data from our characters
        [Header("Transforms")]
        [SerializeField] private Transform[] eyes;
        [SerializeField] private Transform head;
        
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

        public new Rigidbody rigidbody { get; private set; }

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
                if(_curHealth <= 0) Die(Vector3.zero);
            }
        }
        public float DamageRes => Mathf.Min(0.9f, stats.BaseResistance + _currentResistance);
        public float HealthPercent => Health / stats.Hp;

        #endregion

        //This dictionary contains state information
        private readonly Dictionary<uint, int> _data = new();
        public int GetDataDictionaryValue(uint key) => _data[key];
        //We want this as a function so we can bind information to keys.
        public void SetDataDictionaryValue(uint key, int value) => _data[key] = value; 
        public bool TryAddDataKey(uint key, int initialValue) =>  _data.TryAdd(key, initialValue); 
        
        #region Actions

        //-------------- Actions -----------//
        //Austin, add more if needed... These are observables used in Effects, Abilities, AI and UI
        //When might we need to indicate to one of those subsystem that something has happened?
        //For instance; if the AI takes damage (OnHealthUpdated) maybe it gets scared and runs away.
        public event Action<float> OnHealthUpdated;
        public event Action OnDeath;
        
        #endregion

        private void Awake()
        {
            InitializeComponents();
           // InitializeAbilities();
        }

        protected virtual void InitializeComponents()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        /*
        protected virtual void InitializeAbilities()
        {
            foreach (GenericAbility ability in stats.DefaultAbilities)
            {
                GenericAbility spawned = Instantiate(ability, transform);
                spawned.NetworkObject.SpawnWithOwnership(OwnerClientId);
            }
        }
        */
        
        
        #region Damagable
        public virtual void OnHurt(float amount, Vector3 force) 
        {
            OnHealthUpdated?.Invoke(amount);
            rigidbody.AddForce(force, ForceMode.Impulse);
            Debug.Log($"{name} was hit for {amount}",gameObject);
        }

        public virtual void Die(Vector3 force)
        {
            Debug.Log($"{name} Died",gameObject);
            OnDeath?.Invoke();
        }
        #endregion

    }
}