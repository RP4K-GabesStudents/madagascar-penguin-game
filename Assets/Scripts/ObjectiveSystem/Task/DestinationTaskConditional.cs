using System;
using ObjectiveSystem.Core;

namespace ObjectiveSystem.Task
{
    public class DestinationTaskConditional : ITaskConditional
    {
        public DestinationTaskConditional(string taskName, bool optional)
        {
            TaskName = taskName;
            Optional = optional;
        }

        public bool Optional { get; }
        public event Action OnComplete;
        public string TaskName { get; }
        ETaskState ITaskConditional.currentState { get; set; } = ETaskState.Active;

        public void Dispose()
        {
            // TODO release managed resources here
        }
        
        public string GetDescription()
        {
            return "DestinationTaskConditional";
        }
    }
}
