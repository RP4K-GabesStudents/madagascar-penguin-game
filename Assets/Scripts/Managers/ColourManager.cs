using System;
using UnityEngine;

namespace Managers
{
    public static class ColourManager
    {
        public static float ColourChangeSpeed = 0.05f;
        private static Color _cachedColour = Color.red;
        private static float _lastUpdateTime = -1f;
        
        public static Color CurColour
        {
            get
            {
                UpdateColour();
                return _cachedColour;
            }
        }

        private static void UpdateColour()
        {
            float currentTime = Time.time;
            
            // Initialize on first access
            if (_lastUpdateTime < 0f)
            {
                _lastUpdateTime = currentTime;
                _cachedColour = Color.red;
                return;
            }
            
            // Use HSV for smooth rainbow transition
            // Hue cycles from 0 to 1 (red -> red) over time
            float hue = (currentTime * ColourChangeSpeed);
            _cachedColour = GetColourAtHue(hue);
            
            _lastUpdateTime = currentTime;
        }
        
        /// <summary>
        /// Resets the colour manager to its initial state
        /// </summary>
        public static void Reset()
        {
            _lastUpdateTime = -1f;
            _cachedColour = Color.red;
        }
        
        /// <summary>
        /// Gets the colour at a specific hue value (0-1)
        /// </summary>
        public static Color GetColourAtHue(float hue)
        {
            return Color.HSVToRGB(hue % 1f, 1f, 1f);
        }
    }
}