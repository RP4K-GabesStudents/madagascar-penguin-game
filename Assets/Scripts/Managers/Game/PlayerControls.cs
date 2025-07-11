using Game.Characters;
using UnityEngine;
using Utilities.General;

public static class PlayerControls
{
   private static readonly GameControls Controls;
   private static IControllable _controlled;
   private static int _currentInventorySlot;

   // ReSharper disable PossibleNullReferenceException
   static PlayerControls() 
   {
      Controls = new GameControls();
      
      Controls.Player.Attack.performed += ctx => _controlled.SetAttackState(ctx.ReadValueAsButton());
      Controls.Player.Sliding.performed += ctx => _controlled.SetCrouchState(ctx.ReadValueAsButton());
      Controls.Player.Interact.performed += ctx => _controlled.SetInteractState(ctx.ReadValueAsButton());
      Controls.Player.Jump.performed += ctx => _controlled.SetJumpState(ctx.ReadValueAsButton());
      Controls.Player.Sprint.performed += ctx => _controlled.SetSprintState(ctx.ReadValueAsButton());
      Controls.Player.Move.performed += ctx => _controlled.SetMoveDirection(ctx.ReadValue<Vector2>());
      Controls.Player.Look.performed += ctx => _controlled.SetLookDirection(ctx.ReadValue<Vector2>());
         
      Controls.Player.HotBarSlot1.performed += _ => SetSlot(0);
      Controls.Player.HotBarSlot2.performed += _ => SetSlot(1);
      Controls.Player.HotBarSlot3.performed += _ => SetSlot(2);
      Controls.Player.HotBarSlot4.performed += _ => SetSlot(3);
      Controls.Player.HotBarSlot5.performed += _ => SetSlot(4);
      Controls.Player.HotBarScroll.performed += ctx => SetSlot(ctx.ReadValue<float>().NormalizeToInt() + _currentInventorySlot, true);

   }

   public static void SetSlot(int slot, bool wrap = false)
   {
      //If we cannot wrap, and the slot is greater than the number of slots, or negative... Cancel
      if (!wrap && (slot >= _controlled.GetNumInventorySlots() || slot < 0)) return;

      if (slot < 0) _currentInventorySlot = _controlled.GetNumInventorySlots() - 1;
      else if (slot > _currentInventorySlot) _currentInventorySlot = 0;

      _controlled.TrySetInventorySlot(_currentInventorySlot);
   }

   public static void BindPlayer(IControllable controlled)
   {
      _controlled = controlled;
      SetSlot(0);
   }

   public static void EnableGame()
   {
      Controls.Player.Enable();
      Controls.UI.Disable();
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
      
   }

   public static void EnableUi()
   {     
      Controls.Player.Disable();
      Controls.UI.Enable();
      Cursor.lockState = CursorLockMode.Confined;
      Cursor.visible = true;
   }
   
}
