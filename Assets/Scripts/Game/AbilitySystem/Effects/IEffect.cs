using Game.Characters;
using Game.Characters.World;

namespace Game.AbilitySystem.Effects
{
    public interface IEffect
    {
        public void OnEffectActivated(GenericCharacter player);
        public void OnEffectDeactivated(GenericCharacter player);
    }
}