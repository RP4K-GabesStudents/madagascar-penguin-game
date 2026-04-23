using System;
using ObjectiveSystem.Core;
using UnityEngine;

namespace ObjectiveSystem.Task
{
    // ─────────────────────────────────────────────────────────────────────────
    // Kill
    // ─────────────────────────────────────────────────────────────────────────

    [Serializable]
    public class KillConditionalData : TaskConditionalData
    {
        [Tooltip("Fully qualified type name of the ITaskObservable target (e.g. MyGame.Enemies.Grunt)")]
        [SerializeField] private string observableTypeName = "";
        [SerializeField] private int requiredKills = 1;

        public override string EditorLabel =>
            $"Kill {requiredKills}x {ShortTypeName(observableTypeName)}";

        public override ITaskConditional BuildConditional()
        {
            var targetType = Type.GetType(observableTypeName);
            if (targetType == null)
            {
                Debug.LogError($"[KillConditionalData] Could not resolve type '{observableTypeName}'");
                return null;
            }

            var genericType = typeof(KillConditional<>).MakeGenericType(targetType);
            return (ITaskConditional)Activator.CreateInstance(genericType, optional, taskName, requiredKills);
        }

        private static string ShortTypeName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "<?>";
            var parts = fullName.Split('.');
            return parts[parts.Length - 1];
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Interaction
    // ─────────────────────────────────────────────────────────────────────────

    [Serializable]
    public class InteractionConditionalData : TaskConditionalData
    {
        [Tooltip("Fully qualified type name of the IInteractObservable target")]
        [SerializeField] private string observableTypeName = "";
        [SerializeField] private int requiredInteractions = 1;

        public override string EditorLabel =>
            $"Interact {requiredInteractions}x with {ShortTypeName(observableTypeName)}";

        public override ITaskConditional BuildConditional() =>
            new InteractionConditional(optional, taskName, observableTypeName, requiredInteractions);

        private static string ShortTypeName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "<?>";
            var parts = fullName.Split('.');
            return parts[parts.Length - 1];
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Destination
    // ─────────────────────────────────────────────────────────────────────────

    [Serializable]
    public class DestinationConditionalData : TaskConditionalData
    {
        [Tooltip("Tag of the destination trigger zone")]
        [SerializeField] private string destinationTag = "Destination";
        [Tooltip("Optional waypoint marker name shown to the player")]
        [SerializeField] private string waypointLabel = "";

        public override string EditorLabel =>
            string.IsNullOrEmpty(waypointLabel)
                ? $"Reach '{destinationTag}'"
                : $"Reach '{waypointLabel}'";

        public override ITaskConditional BuildConditional() =>
            new DestinationConditional(taskName, optional, destinationTag, waypointLabel);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Stealth
    // ─────────────────────────────────────────────────────────────────────────

    [Serializable]
    public class StealthConditionalData : TaskConditionalData
    {
        [Tooltip("Fail the conditional the moment the player is spotted (vs. tracking a count)")]
        [SerializeField] private bool failOnFirstSpot = true;
        [Tooltip("Maximum allowed times the player can be spotted (ignored if failOnFirstSpot is true)")]
        [SerializeField] private int allowedSpotCount = 0;

        public override string EditorLabel =>
            failOnFirstSpot ? "Not spotted" : $"Spotted ≤ {allowedSpotCount}x";

        public override ITaskConditional BuildConditional() =>
            new StealthConditional(optional, taskName, failOnFirstSpot, allowedSpotCount);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Time
    // ─────────────────────────────────────────────────────────────────────────

    [Serializable]
    public class TimeConditionalData : TaskConditionalData
    {
        public enum TimeMode { CompleteWithin, SurviveFor }

        [SerializeField] private TimeMode mode = TimeMode.CompleteWithin;
        [SerializeField, Min(0f)] private float seconds = 30f;

        public override string EditorLabel => mode switch
        {
            TimeMode.CompleteWithin => $"Within {seconds}s",
            TimeMode.SurviveFor     => $"Survive {seconds}s",
            _                       => "Time condition"
        };

        public override ITaskConditional BuildConditional() =>
            new TimeConditional(optional, taskName, mode == TimeConditionalData.TimeMode.SurviveFor, seconds);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Scene Change
    // ─────────────────────────────────────────────────────────────────────────

    [Serializable]
    public class SceneChangeConditionalData : TaskConditionalData
    {
        [Tooltip("Name of the target scene (leave empty to trigger on any scene change)")]
        [SerializeField] private string targetSceneName = "";

        public override string EditorLabel =>
            string.IsNullOrEmpty(targetSceneName)
                ? "Any scene change"
                : $"Enter scene '{targetSceneName}'";

        public override ITaskConditional BuildConditional() =>
            new SceneChangeConditional(optional, taskName, targetSceneName);
    }
}
