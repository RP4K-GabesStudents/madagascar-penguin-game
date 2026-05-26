using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using UnityEngine;

namespace Network.Sequences.Create
{
    public class CreateLobbyFailSequence : MonoBehaviour, IEntrySequence
    {
        private void Awake()
        {
            Debug.LogError("Lobby failed to create");
        }

        public event Action<string> DisplayMessage;
        public UniTask<IEntrySequence> ExecuteSequence()
        {
            throw new NotImplementedException();
        }

        public IEntrySequence Default { get; }
        public bool IsCompleted { get; }
    }
}
