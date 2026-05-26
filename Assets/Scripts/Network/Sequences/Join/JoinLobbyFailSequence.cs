using System;
using UnityEngine;

namespace Network.Sequences.Join
{
    public class JoinLobbyFailSequence : MonoBehaviour
    {
        private void Awake()
        {
            Debug.LogError("Failed to join lobby ");
        }
    }
}
