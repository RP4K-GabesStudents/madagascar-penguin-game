using System;

namespace ObjectiveSystem.Core
{
    public interface ITask : IDisposable
    {
        public bool Optional { get; }
        public event Action OnComplete;
        public string TaskName { get; }
        
        
        public bool IsComplete();
        
    }
}
