using System;

namespace ObjectiveSystem.Core
{
    public interface ITaskConditional : IDisposable
    {
        public bool Optional { get; }
        public event Action OnComplete;
        public string TaskName { get; }

        // public bool DidFail();
        // public bool IsComplete();
        protected internal ETaskState currentState { get;  set; }
        public ETaskState CurrentState => currentState;
    }

    public enum ETaskState
    {
        Active,
        Failed,
        Successful
    }
}
