using System;
using ObjectiveSystem.Core;

namespace ObjectiveSystem.Task
{
    public class TimeTask : ITask
    {
        public TimeTask(bool optional, string taskName)
        {
            Optional = optional;
            TaskName = taskName;
        }

        public bool Optional { get; }
        public event Action OnComplete;
        public string TaskName { get; }
        public bool IsComplete()
        {
            throw new NotImplementedException();
        }
    }
}
