using System;
using ObjectiveSystem.Core;
using UnityEngine;

namespace ObjectiveSystem.Task
{
    public class StealthTaskConditional : ITaskConditional
    {
        public StealthTaskConditional(bool optional, string taskName)
        {
            Optional = optional;
            TaskName = taskName;
        }

        public bool Optional { get; }
        public event Action OnComplete;
        public string TaskName { get; }
        ETaskState ITaskConditional.currentState { get; set; } = ETaskState.Active;
        public string GetDescription()
        {
            return "StealthTaskConditional";
        }


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
