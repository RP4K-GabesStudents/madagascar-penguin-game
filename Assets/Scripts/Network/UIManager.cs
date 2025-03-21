using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Network
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] Button hostButton;
        [SerializeField] Button joinButton;
        
        [SerializeField] public TMP_InputField nameInputField;
    }
}
