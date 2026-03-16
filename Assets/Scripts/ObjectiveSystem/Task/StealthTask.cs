using System;
using ObjectiveSystem.Core;
using UnityEngine;

namespace ObjectiveSystem.Task
{
    public class StealthTask : ITask
    {
        public StealthTask(bool optional, string taskName)
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

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
