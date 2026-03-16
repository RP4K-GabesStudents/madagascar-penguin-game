using System;

namespace ObjectiveSystem.Core
{
    public static class TaskObservableEventBus<T> where T : ITaskObservable
    {
        public static event Action<EActionType> OnActionSubmitted;
        public static void Publish(EActionType actionType) => OnActionSubmitted?.Invoke(actionType);
    }
}
