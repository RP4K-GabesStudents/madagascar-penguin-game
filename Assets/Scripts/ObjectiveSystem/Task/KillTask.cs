using System;
using ObjectiveSystem.Core;

namespace ObjectiveSystem.Task
{
    public class KillTask<T> : ITask where T : ITaskObservable
    {
        
        public KillTask(bool optional, string taskName, T target, int amount)
        {
            Optional = optional;
            TaskName = taskName;
            target.OnComplete += CheckEnemyDeath;
        }

        private void CheckEnemyDeath(ITaskObservable.EActionType obj)
        {
            if (obj == ITaskObservable.EActionType.KillEvent)
            {
                
            }
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
