using System;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "HoverInfoStats", menuName = "Scriptable Objects/HoverInfoStats")]
    public class HoverInfoStats : ScriptableObject
    {
        [SerializeField, TextArea] protected string hoverInfoName;
        
        //public string FormattedString { get; private set; }
        public string GetFormattedString()
        {
            string input = InputControlPath.ToHumanReadableString(
                PlayerControls.GameControls.Player.Interact.bindings[0].effectivePath,
                InputControlPath.HumanReadableStringOptions.UseShortNames);
            input = input.Replace(" [Keyboard]", String.Empty);
            string str = hoverInfoName.Replace("{input}", "<color=#"+ColorUtility.ToHtmlStringRGB(ColourManager.CurColour)+">[" + input + "]</color>");
            return str;
        }
    }
}
