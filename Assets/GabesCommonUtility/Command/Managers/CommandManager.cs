using System;
using System.Collections.Generic;
using Commands.Core;
using UnityEngine;

#if USE_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
#else
using System.Collections;
using UnityEngine;
#endif

namespace Commands.Managers
{
    public static class CommandManager
    {
        private static readonly List<ICommand> UndoStack = new();
        private static readonly Stack<ICommand> RedoStack = new();
        
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public static int MaxStackSize = 100;

        public static bool CanRedo() => RedoStack.Count != 0;
        public static bool CanUndo() => UndoStack.Count != 0;

        public static event Action OnUndo = delegate { };
        public static event Action OnRedo = delegate { };
        public static event Action OnExecute = delegate { };
        
#if USE_UNITASK
        private static readonly SemaphoreSlim Lock = new(1, 1);

        public static async UniTask ExecuteAsync(ICommand command, CancellationToken ct = default)
        {
            await Lock.WaitAsync(ct);
            try
            {
                await command.ExecuteAsync(ct);
                PushToUndo(command);
                RedoStack.Clear();
                OnExecute?.Invoke();
            }
            finally { Lock.Release(); }
        }

        public static async UniTask UndoAsync(CancellationToken ct = default)
        {
            if (!CanUndo())
            {
                Debug.Log("Nothing to undo");
                return;
            }
            await Lock.WaitAsync(ct);
            try
            {
                int lastIndex = UndoStack.Count - 1;
                ICommand command = UndoStack[lastIndex];
                UndoStack.RemoveAt(lastIndex);
                Debug.Log("Undo: " + command.DisplayName);
                await command.UndoAsync(ct);
                RedoStack.Push(command);
                OnUndo?.Invoke();
            }
            finally { Lock.Release(); }
        }

        public static async UniTask RedoAsync(CancellationToken ct = default)
        {
            
            await Lock.WaitAsync(ct);
            if (!CanRedo())
            {
                Debug.Log("Nothing to redo");
                return;
            }
            try
            {
                ICommand command = RedoStack.Pop();
                Debug.Log("Redo: " + command.DisplayName);
                await command.ExecuteAsync(ct);
                PushToUndo(command);
                OnRedo?.Invoke();
            }
            finally { Lock.Release(); }
        }

#else
        private static bool _isProcessing;
        private class RoutineRunner : MonoBehaviour { }
        private static RoutineRunner _runner;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var go = new GameObject("CommandRunner") { hideFlags = HideFlags.HideInHierarchy };
            _runner = go.AddComponent<RoutineRunner>();
            Object.DontDestroyOnLoad(go);
        }

        public static void Execute(ICommand command)
        {
            if (!_isProcessing) _runner.StartCoroutine(DoExecute(command));
        }

        public static void Undo()
        {
            if (!_isProcessing && UndoStack.Count > 0) _runner.StartCoroutine(DoUndo());
        }

        public static void Redo()
        {
            if (!_isProcessing && RedoStack.Count > 0) _runner.StartCoroutine(DoRedo());
        }

        private static IEnumerator DoExecute(ICommand command)
        {
            _isProcessing = true;
            yield return command.Execute();
            PushToUndo(command);
            RedoStack.Clear();
            _isProcessing = false;
        }

        private static IEnumerator DoUndo()
        {
            _isProcessing = true;
            int lastIndex = UndoStack.Count - 1;
            ICommand command = UndoStack[lastIndex];
            UndoStack.RemoveAt(lastIndex);

            yield return command.Undo();
            RedoStack.Push(command);
            _isProcessing = false;
        }

        private static IEnumerator DoRedo()
        {
            _isProcessing = true;
            ICommand command = RedoStack.Pop();
            yield return command.Execute();
            PushToUndo(command);
            _isProcessing = false;
        }
#endif

        private static void PushToUndo(ICommand command)
        {
            UndoStack.Add(command);
            if (UndoStack.Count > MaxStackSize)
            {
                UndoStack.RemoveAt(0);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }
    }
}