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
