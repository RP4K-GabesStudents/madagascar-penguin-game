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
        public ETaskState GetCurrentState();

        public string GetDescription();
    }

    public enum ETaskState
    {
        Active,
        Failed,
        Successful
    }
}
