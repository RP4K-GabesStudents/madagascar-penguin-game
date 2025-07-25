using System;
using Inventory;
using Managers;
using Unity.Netcode;
using UnityEngine;
using Utilities;

namespace penguin
{
    [SelectionBase]
    public abstract class BasePenguinFile : NetworkBehaviour
    {
        public abstract float GetHorizontalSpeed();
        protected IKController _ikController;
        protected Animator _animator;
        private Item _currentlyHeld;

        protected virtual void Awake()
        {
            _ikController = GetComponentInChildren<IKController>();
            _animator = GetComponent<Animator>();
        }

        protected virtual void LateUpdate()
        {
            HandleAnimation();
        }
        protected virtual void HandleAnimation()
        {
            _animator.SetFloat(StaticUtilities.ForwardAnimID, GetHorizontalSpeed());
        }

        
    }
}