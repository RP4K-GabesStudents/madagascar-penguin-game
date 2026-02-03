using System.Threading;
using Commands.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Command.Common
{
    public class EnableDisableComponentCommand : ICommand
    {
        private readonly Behaviour _behaviour;
        private readonly bool _originalState;
        private readonly bool _newState;

        public EnableDisableComponentCommand(Behaviour behaviour, bool newState)
        {
            _behaviour = behaviour;
            _originalState = !newState;
            _newState = newState;
        }

        public string DisplayName => $"{(_newState ? "Enable" : "Disable")} {_behaviour.GetType().Name}";
        
        public UniTask ExecuteAsync(CancellationToken ct = default)
        {
            _behaviour.enabled = _newState;
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct = default)
        {
            _behaviour.enabled = _originalState;
            return UniTask.CompletedTask;
        }
    }
}