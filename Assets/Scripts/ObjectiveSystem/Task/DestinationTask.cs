using System;
using ObjectiveSystem.Core;

namespace ObjectiveSystem.Task
{
    public class DestinationTask : ITask
    {
        public DestinationTask(string taskName, bool optional)
        {
            TaskName = taskName;
            Optional = optional;
        }

        public bool Optional { get; }
        public event Action OnComplete;
        public string TaskName { get; }
        public bool IsComplete()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
