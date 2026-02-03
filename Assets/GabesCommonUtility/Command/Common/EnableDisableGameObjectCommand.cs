using System.Threading;
using Commands.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Command.Common
{
    public class EnableDisableGameObjectCommand : ICommand
    {
        private readonly GameObject _gameObject;
        private readonly bool _originalState;
        private readonly bool _newState;

        public EnableDisableGameObjectCommand(GameObject gameObject, bool newState)
        {
            _gameObject = gameObject;
            _originalState = !newState;
            _newState = newState;
        }

        public string DisplayName => $"{(_newState ? "Enable" : "Disable")} {_gameObject.name}";
        
        public UniTask ExecuteAsync(CancellationToken ct = default)
        {
            _gameObject.SetActive(_newState);
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct = default)
        {
            _gameObject.SetActive(_originalState);
            return UniTask.CompletedTask;
        }
    }
}