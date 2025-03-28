using System;
using Managers;
using penguin;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.Serialization;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private HoverInfoStats hoverInfoStats;
    public void DisplayMessage()
    {
        ToolTipManager.Instance.ShowToolTip(hoverInfoStats);
    }
    public void StopDisplayingMessage()
    {
        ToolTipManager.Instance.HideToolTip();
    }
}
