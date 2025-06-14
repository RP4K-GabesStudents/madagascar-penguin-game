using System;
using Managers;
using Scriptable_Objects;

namespace Interfaces
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
        public void OnInteract();
        public HoverInfoStats GetHoverInfoStats();
        public bool CanHover();
    }
}