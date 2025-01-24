using UnityEngine;

namespace Interfaces
{
    public interface IControllable
    {
        public void OnShift(bool state);
        public void OnMove(Vector2 direction);
        public void OnJump(bool state);
        public void OnCrouch(bool state);
        public void OnAttack(bool state);
        
    }
}