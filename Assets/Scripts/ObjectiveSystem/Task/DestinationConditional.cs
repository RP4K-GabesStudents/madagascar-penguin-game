using System;
using ObjectiveSystem.Core;

namespace ObjectiveSystem.Task
{
    public class DestinationConditional : ITaskConditional
    {
        public DestinationConditional(string taskName, bool optional,
            string destinationTag, string waypointLabel = "")
        {
            TaskName = taskName;
            Optional = optional;
            DestinationTag = destinationTag;
            WaypointLabel = waypointLabel;
        }

        public string DestinationTag { get; }
        public string WaypointLabel { get; }

        public bool Optional { get; }
        public string TaskName { get; }
        ETaskState currentState { get; set; } = ETaskState.Active;
        public event Action OnComplete;

        /// <summary>
        /// Call this from your trigger zone when the player enters.
        /// Filter by DestinationTag before calling.
        /// </summary>
        public void OnPlayerArrived()
        {
            currentState = ETaskState.Successful;
            OnComplete?.Invoke();
        }

        public ETaskState GetCurrentState()
        {
            return currentState;
        }

        public string GetDescription() =>
            string.IsNullOrEmpty(WaypointLabel)
                ? $"Reach destination ({DestinationTag})"
                : $"Go to {WaypointLabel}";

        public void Dispose() { }
    }
}
