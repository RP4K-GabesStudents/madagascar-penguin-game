using System;
using System.Collections.Generic;
using System.Linq;
using ObjectiveSystem.Core;

namespace ObjectiveSystem.Task
{
    public class TaskConditionalGroupTaskConditional : ITaskConditional
    {
        public TaskConditionalGroupTaskConditional( bool optional)
        {
            
            Optional = optional;
        }

        public bool Optional { get; }
        public event Action OnComplete;
        public string TaskName { get; }

        ETaskState ITaskConditional.currentState { get; set; } = ETaskState.Active;
        private readonly HashSet<ITaskConditional> _tasks = new();
        private ETaskState _currentState;
        private ETaskState _currentState1;

        // public bool IsComplete()
        // {
        //     return (!Optional && _tasks.All(task => task.IsComplete())) || Optional;
        // }

        public bool AddTask(ITaskConditional taskConditional)
        {
            if (!_tasks.Add(taskConditional)) return false;

            taskConditional.OnComplete += CheckComplete;
            return true;
        }

        private void CheckComplete()
        {
            // if (IsComplete())
            // {
            //     OnComplete?.Invoke();
            // }
        }
        
        public string GetDescription()
        {
            return "TaskConditionalGroupTaskConditional";
        }

        public bool RemoveTask(ITaskConditional taskConditional)
        {
            if (_tasks.Remove(taskConditional)) return false;
            taskConditional.OnComplete -= CheckComplete;
            return true;
        }

        public HashSet<ITaskConditional> GetTaskList()
        {
            return _tasks;
        }

        public void Dispose()
        {
            
        }
    }
}
