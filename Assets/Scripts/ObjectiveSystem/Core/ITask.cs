using System;

namespace ObjectiveSystem.Core
{
    public interface ITask
    {
        public bool Optional { get; }
        public event Action OnComplete;
        public string TaskName { get; }
        
        
        public bool IsComplete();
        
    }
}
