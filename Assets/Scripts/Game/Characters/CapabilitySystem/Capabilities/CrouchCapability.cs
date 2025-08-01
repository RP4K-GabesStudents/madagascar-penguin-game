using System;
using Game.Characters.Movement;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    public class CrouchCapability : BaseCapability, IInputSubscriber
    {
        private bool inputState;
        protected override void OnBound()
        {
            
        }

        public override bool CanExecute()
        {
            return owner.GetDataDictionaryValue(CapabilityKeys.IsCrouching).IntAsBool() && IsTopClear();
        }

        protected override void Execute()
        {
            
        }

        private void FixedUpdate()
        {
            //bool hit = Physics.SphereCast()
        }

        private bool IsTopClear()
        {
            Debug.LogWarning("Crouching is incomplete");
            return true;
        }

        public void BindControls(GameControls controls)
        {
            controls.Player.Crouch.performed += ctx =>
            {
                inputState = ctx.ReadValueAsButton();
                TryExecute();
            };
        }
    }
}