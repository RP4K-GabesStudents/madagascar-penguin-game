namespace  ObjectiveSystem.Core 
{
    public enum EActionType
    {
        Kill,
        Spotted,
        Interact
    }

    public interface ITaskObservable { }

    public interface IKillTaskObservable : ITaskObservable
    {
        public EActionType ActionType =>  EActionType.Kill;
    }

    public interface IInteractObservable : ITaskObservable
    {
        public EActionType ActionType =>  EActionType.Spotted;
    }

    public interface ISpottedObservable : ITaskObservable
    {
        public EActionType ActionType =>  EActionType.Interact;
    }
}
