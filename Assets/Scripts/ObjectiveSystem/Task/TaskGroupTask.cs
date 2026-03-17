using System;
using System.Collections.Generic;
using System.Linq;
using ObjectiveSystem.Core;

namespace ObjectiveSystem.Task
{
    public class TaskGroupTask : ITask
    {
        public TaskGroupTask(string taskName, bool optional)
        {
            TaskName = taskName;
            Optional = optional;
        }

        public bool Optional { get; }
        public event Action OnComplete;
        public string TaskName { get; }
        private readonly HashSet<ITask> _tasks = new();

        public bool IsComplete()
        {
            return (!Optional && _tasks.All(task => task.IsComplete())) || Optional;
        }

        public bool AddTask(ITask task)
        {
            if (!_tasks.Add(task)) return false;

            task.OnComplete += CheckComplete;
            return true;
        }

        private void CheckComplete()
        {
            if (IsComplete())
            {
                OnComplete?.Invoke();
            }
        }

        public bool RemoveTask(ITask task)
        {
            if (_tasks.Remove(task)) return false;
            task.OnComplete -= CheckComplete;
            return true;
        }

        public HashSet<ITask> GetTaskList()
        {
            return _tasks;
        }

        public void Dispose()
        {
            
        }
    }
}
