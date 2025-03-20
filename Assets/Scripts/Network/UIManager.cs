using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Network
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] Button _hostButton;
        [SerializeField] Button _joinButton;
        
        [SerializeField] public TMP_InputField nameInputField;
    }
}
