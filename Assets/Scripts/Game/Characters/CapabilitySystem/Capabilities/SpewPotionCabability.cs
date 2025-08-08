using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    public class SpewPotionCapability : BaseCapability, IInputSubscriber
    {
        [SerializeField] private float timer = 15;
        [SerializeField] private int potionAmount = 3;
        [SerializeField] private Rigidbody[] potionPrefabs;
        [SerializeField] private Transform potionSpawnPoints;
        [SerializeField] private float potionSpawnForce;
        
        private bool _isOnCooldown;
        public override bool CanExecute()
        {
            return !_isOnCooldown && owner.GetDataDictionaryValue(CapabilityKeys.IsGrounded).IntAsBool();
            
        }
        protected override void OnBound()
        {
            base.OnBound();
            owner.TryAddDataKey(CapabilityKeys.IsGrounded, false.BoolAsInt());
        }

        protected override void Execute()
        {
            //add random torque and force and spawn potions
            //use variable right
        }

        public void BindControls(GameControls controls)
        {
            //TODO get rid of jump and make keybind and inputs for abilities
            controls.Player.Jump.performed += ctx => TryExecute();
        }
        
    }
}