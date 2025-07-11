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
        public string Input { get; private set; }
        
        public virtual string GetFormattedString()
        {
            string str = hoverInfoName.Replace("{input}", "<color=#"+ColorUtility.ToHtmlStringRGB(ColourManager.CurColour)+">[" + Input + "]</color>");
            return str;
        }

        public void RecompileString()
        {
            Input = InputControlPath.ToHumanReadableString(PlayerControls.GameControls.Player.Interact.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames);
            Input = Input.Replace(" [Keyboard]", String.Empty);
        }


        private void OnEnable()
        {
            RecompileString();
        }
    }
}
