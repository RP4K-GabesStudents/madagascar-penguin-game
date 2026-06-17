using UnityEngine;
using UI.Effects.ScriptableObjects;

namespace UI.Effects
{
    /// <summary>
    /// Loads default animation presets from Resources/CustomUIEffects. Missing
    /// assets return null rather than throwing, so effects degrade to inert.
    /// </summary>
    public static class AnimationEffectDefaults
    {
        private const string Folder = "CustomUIEffects/";

        public const string ScaleKey  = Folder + "DefaultScaleAnimationData";
        public const string ColorKey  = Folder + "DefaultColorAnimationData";
        public const string MoveKey   = Folder + "DefaultMoveAnimationData";
        public const string RotateKey = Folder + "DefaultRotateAnimationData";

        public static T Load<T>(string key) where T : AnimationDataSoBase
        {
            var asset = Resources.Load<T>(key);
            if (asset == null)
                Debug.LogWarning($"[UI.Effects] Default preset '{key}' not found in Resources/CustomUIEffects. Effect is inert until a preset is assigned.");
            return asset;
        }
    }
}