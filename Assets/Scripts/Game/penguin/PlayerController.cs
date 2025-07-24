using Game.Characters;
using Interfaces;
using Inventory;
using Managers;
using Scriptable_Objects;
using Scriptable_Objects.Penguin_Stats;
using UnityEngine;

namespace Game.penguin
{
    
    public class PlayerController : GenericCharacter, IDamageable
    {
        private Player _controlled;
        [SerializeField] PenguinStats penguinStats;
        public PenguinStats PenguinoStats => penguinStats;
        [SerializeField] private ProjectileStats projectileStats;
        public ProjectileStats ProjectileStats => projectileStats;
        [SerializeField] private MorePenguinStats morePenguinStats;
        private Rigidbody rigidbody;
        private float _curHealth;
        private Item _curItem;
        

        [Header("Jumping")] 
    
    
   


        



      

        protected override void LateUpdate()
        {
            base.LateUpdate();
            HandleLooking();
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
            rigidbody.AddForce(transform.up * penguinStats.JumpPower, ForceMode.Impulse);
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
            _curItem?.TryUseItem();
            OnAttack?.Invoke();
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
        
        

      

      

    }
}
