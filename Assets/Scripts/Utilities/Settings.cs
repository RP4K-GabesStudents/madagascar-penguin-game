using System;
using UnityEngine;

namespace Utilities
{
    public static class Settings
    {
        public static event Action OnSettingsChanged;
        public static GamePlaySettings GamePlaySettings { get; private set; }

        public static void Save()
        {
            OnSettingsChanged?.Invoke();
        }

        public static void Load()
        {
            
        }
    }

    [Serializable]
    public class GamePlaySettings
    {
        
        public bool autoEquip = true;
    }
}
