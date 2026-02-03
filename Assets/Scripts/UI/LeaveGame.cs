using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class LeaveGame : MonoBehaviour
    {
        public void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();   
#endif
        }
    }
}
