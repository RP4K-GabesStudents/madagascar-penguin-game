using System;

namespace Utilities
{
    public static class Settings
    {
        public static event Action OnSettingsChanged;
        public static GamePlaySettings GamePlaySettings { get; private set; } = new GamePlaySettings();

        public static void Save()
        {
            OnSettingsChanged?.Invoke();
        }

        public static void Load()
        {
            
        }
    }

    [Serializable]
    public struct GamePlaySettings
    {
        public bool autoEquip;
        public GamePlaySettings(string profile)
        {
            autoEquip = true;
        }
    }
}
