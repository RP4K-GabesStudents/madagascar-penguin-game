using System;

namespace  ObjectiveSystem.Core 
{
    public interface ITaskObservable 
    {
        public event Action<EActionType> OnComplete;
        
        public enum EActionType
        {
            KillEvent,
            ActionEvent,
            SpottedEvent,
            InteractEvent
        }
    }
}
