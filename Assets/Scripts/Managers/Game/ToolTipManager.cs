using System;
using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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
            //canvas.enabled = true;
            enabled = stats;
        }
        private void LateUpdate()
        {
            toolTipText.text = _currentHoverInfo.GetFormattedString();
        }
        public void HideToolTip()
        {
            enabled = false;
            //canvas.enabled = false;
            toolTipText.text = string.Empty;
        }
    }
}
