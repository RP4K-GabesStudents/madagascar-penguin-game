using UnityEngine;

public static class PlayerControls
{
   private static readonly GameControls gameControls = new GameControls();
   private static Player _penguin;
   private static bool _isPenguinBound;
   
   public static GameControls GameControls => gameControls;

   public static void BindPlayer(Player penguin)
   {
      if(!_isPenguinBound)
      {
         GameControls.Player.Attack.performed += ctx => _penguin.Attack(ctx.ReadValueAsButton());
         GameControls.Player.Sliding.performed += ctx => _penguin.Sliding(ctx.ReadValueAsButton());
         GameControls.Player.Interact.performed += ctx => _penguin.Interact(ctx.ReadValueAsButton());
         GameControls.Player.Jump.performed += ctx => _penguin.Jump(ctx.ReadValueAsButton());
         GameControls.Player.Sprint.performed += ctx => _penguin.Sprint(ctx.ReadValueAsButton());
         GameControls.Player.Move.performed += ctx => _penguin.SetMoveDirection(ctx.ReadValue<Vector2>());
         GameControls.Player.Look.performed += ctx => _penguin.Look(ctx.ReadValue<Vector2>());
         
         GameControls.Player.HotBarSlot1.performed += _ => _penguin.SetSelected(0);
         GameControls.Player.HotBarSlot2.performed += _ => _penguin.SetSelected(1);
         GameControls.Player.HotBarSlot3.performed += _ => _penguin.SetSelected(2);
         GameControls.Player.HotBarSlot4.performed += _ => _penguin.SetSelected(3);
         GameControls.Player.HotBarSlot5.performed += _ => _penguin.SetSelected(4);
         GameControls.Player.HotBarScroll.performed += ctx => _penguin.ScrollSelected(ctx.ReadValue<float>());
      }

      _isPenguinBound = true;
      _penguin = penguin;
   }

   public static void BindCinematic()
   {
      
   }

   public static void EnableGame()
   {
      GameControls.Player.Enable();
      Cursor.lockState = CursorLockMode.Confined;
      Cursor.visible = false;
      
   }

   public static void EnableUi()
   {
      
   }

   public static void EnableCutscene()
   {
      
   }
   
}
