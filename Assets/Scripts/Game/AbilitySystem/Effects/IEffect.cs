using Game.penguin;

namespace Game.AbilitySystem.Effects
{
    public interface IEffect
    {
        public void OnEffectActivated(PlayerController player);
        public void OnEffectDeactivated(PlayerController player);
    }
}