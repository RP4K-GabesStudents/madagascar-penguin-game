using System;
using System.Threading;
using Commands.Core;
using Cysharp.Threading.Tasks;

namespace Command.Common
{
    public class BasicSwapModificationCommand<T> : ICommand
    {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;
        private readonly T _originalValue;
        private readonly T _newValue;

        public BasicSwapModificationCommand(Func<T> getter, Action<T> setter, T newValue)
        {
            _getter = getter;
            _setter = setter;
            _originalValue = getter();
            _newValue = newValue;
        }

        public string DisplayName => "SimpleSwapModification";
        
        public UniTask ExecuteAsync(CancellationToken ct = default)
        {
            _setter(_newValue);
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct = default)
        {
            _setter(_originalValue);
            return UniTask.CompletedTask;
        }
    }
}