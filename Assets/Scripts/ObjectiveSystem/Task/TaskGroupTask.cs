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
        private readonly HashSet<ITask> Tasks = new();

        public bool IsComplete()
        {
            return (!Optional && Tasks.All(task => task.IsComplete())) || Optional;
        }

        public bool AddTask(ITask task)
        {
            if (!Tasks.Add(task)) return false;

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
            if (Tasks.Remove(task)) return false;
            task.OnComplete -= CheckComplete;
            return true;
        }

        public HashSet<ITask> GetTaskList()
        {
            return Tasks;
        }

        public void Dispose()
        {
            
        }
    }
}
