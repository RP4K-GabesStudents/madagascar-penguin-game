using Game.Characters;
using Game.Characters.World;
using Managers;
using Scriptable_Objects;

namespace Game.Objects
{
    public interface IInteractable
    {
        public void OnHoverDriver()
        {
            ToolTipManager.Instance.ShowToolTip(GetHoverInfoStats());
            OnHover();
        }

        public void OnHoverEndDriver()
        {
            ToolTipManager.Instance.HideToolTip();
            OnHoverEnd();
        }

        public void OnHover();
        public void OnHoverEnd();
        public void OnInteract(GenericCharacter oner);
        public HoverInfoStats GetHoverInfoStats();
        public bool CanHover();
    }
}