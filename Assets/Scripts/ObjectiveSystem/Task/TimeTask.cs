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
        ETaskState ITask.currentState { get; set; } = ETaskState.Active;

        public void Dispose()
        {
            
        }
    }
}
