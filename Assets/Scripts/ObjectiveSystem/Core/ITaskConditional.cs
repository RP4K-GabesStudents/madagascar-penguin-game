using System;

namespace ObjectiveSystem.Core
{
    public delegate void OnUpdateDelegate();

    public interface ITaskConditional : IDisposable
    {
        public bool Optional { get; }
        // public bool DidFail();
        // public bool IsComplete();
        public ETaskState GetCurrentState();

        public string GetDescription();
        
        
        public OnUpdateDelegate OnUpdate { get; set; }

        public void Update(float deltaTime) { }

    }

    public enum ETaskState
    {
        Active,
        Failed,
        Successful
    }
}
