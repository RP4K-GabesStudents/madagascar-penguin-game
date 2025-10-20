using Scriptable_Objects;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class ToolTipManager : MonoBehaviour
    {
        public static ToolTipManager Instance { get; private set; }
        [SerializeField] private TextMeshProUGUI toolTipText;
        HoverInfoStats _currentHoverInfo;
        [SerializeField] private Canvas canvas;
        
        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            HideToolTip();
        }

        public void ShowToolTip(HoverInfoStats stats)
        {
            
            _currentHoverInfo = stats;
            
            if (!stats)
            {
                HideToolTip();
                return;
            }
            
            canvas.enabled = true;
            enabled = _currentHoverInfo.StringNeedsFormatting(out string s);
            toolTipText.text = s;
        }
        private void LateUpdate()
        {
            //VERY EXPENSIVE FUNCTION, JUST TO MAKE IT RAINBOW.
            toolTipText.text = _currentHoverInfo.GetFormattedString();
        }
        public void HideToolTip()
        {
            enabled = false;
            canvas.enabled = false;
        }
    }
}
