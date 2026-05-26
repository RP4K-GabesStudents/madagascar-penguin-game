using System;
using UnityEngine;

namespace Network.Sequences.Create
{
    public class CreateLobbyFailSequence : MonoBehaviour
    {
        private void Awake()
        {
            Debug.LogError("Lobby failed to create");
        }
    }
}
