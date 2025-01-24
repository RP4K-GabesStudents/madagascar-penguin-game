using UnityEngine;

public static class PlayerControls
{
   private static GameControls _gameControls = new GameControls();
   private static Player _penguin;
   private static bool _isPenguinBound;

   public static void BindPlayer(Player penguin)
   {
      if(!_isPenguinBound)
      {
         _gameControls.Player.Attack.performed += ctx => _penguin.Attack(ctx.ReadValueAsButton());
         _gameControls.Player.Crouch.performed += ctx => _penguin.Crouch(ctx.ReadValueAsButton());
         _gameControls.Player.Interact.performed += ctx => _penguin.Interact(ctx.ReadValueAsButton());
         _gameControls.Player.Jump.performed += ctx => _penguin.Jump(ctx.ReadValueAsButton());
         _gameControls.Player.Sprint.performed += ctx => _penguin.Sprint(ctx.ReadValueAsButton());
         _gameControls.Player.Move.performed += ctx => _penguin.SetMoveDirection(ctx.ReadValue<Vector2>());
         _gameControls.Player.Look.performed += ctx => _penguin.Look(ctx.ReadValue<Vector2>());
      }

      _isPenguinBound = true;
      _penguin = penguin;
   }

   public static void BindCinematic()
   {
      
   }

   public static void EnableGame()
   {
      _gameControls.Player.Enable();
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
