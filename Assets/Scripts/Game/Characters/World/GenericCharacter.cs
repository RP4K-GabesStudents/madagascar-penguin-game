using System;
using Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// A Generic Character is like a husk; it describes how a character might work, but not what who controls it, or how it is controlled.
    /// The Generic Character should not query outside scripts
    /// </summary>
    [SelectionBase]
    public class GenericCharacter : NetworkBehaviour, IDamageable
    {
        
        //---------- Transform Information -----------//
        //These objects are useful for retrieving important common data from our characters
        [Header("Transforms")]
        [SerializeField] private Transform[] eyes;
        [SerializeField] private Transform head;
        
        [SerializeField] private Transform headXRotator;
        [SerializeField] private Transform bodyYRotator;

        public Transform[] Eyes => eyes;
        public Transform Head => head;
        
        [Header("")]
        
        public float Health { get;  set; }
        public float DamageRes => 
        
        
        //-------------- Actions -----------//
        //Austin, add more if needed... These are observables used in Effects, Abilities, AI and UI
        //When might we need to indicate to one of those subsystem that something has happened?
        //For instance; if the AI takes damage (OnHealthUpdated) maybe it gets scared and runs away.
        public event Action<float> OnHealthUpdated;
        public event Action OnAttack;
        public event Action OnDeath;
        
        
        public void Die(Vector3 force)
        {
            throw new NotImplementedException();
        }

        public void OnHurt(float amount, Vector3 force)
        {
            throw new NotImplementedException();
        }

      
    }
}