using System;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "HoverInfoStats", menuName = "Scriptable Objects/HoverInfoStats")]
    public class HoverInfoStats : ScriptableObject
    {
        [SerializeField] private InputActionReference reference;
        [SerializeField, TextArea] protected string hoverInfoName;
        public string Input { get; private set; }
        
        public virtual string GetFormattedString()
        {
            string str = hoverInfoName.Replace("{input}", "<color=#"+ColorUtility.ToHtmlStringRGB(ColourManager.CurColour)+">[" + Input + "]</color>");
            return str;
        }

        public bool StringNeedsFormatting(out string formatted)
        {
            formatted = hoverInfoName.Replace("{input}", "<color=#" + ColorUtility.ToHtmlStringRGB(ColourManager.CurColour) + ">[" + Input + "]</color>");
            return formatted != hoverInfoName;
        }

        public void RecompileString()
        {
            if (reference == null) return;
            Input = InputControlPath.ToHumanReadableString(reference.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames);
            Input = Input.Replace(" [Keyboard]", String.Empty);
        }


        private void OnEnable()
        {
            RecompileString();
        }
    }
}
