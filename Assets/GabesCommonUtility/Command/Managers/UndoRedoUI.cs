using System;
using Commands.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class UndoRedoUI : MonoBehaviour
{
    public UnityEvent<bool> onUndoStateChanged;
    public UnityEvent<bool> onRedoStateChanged;

    private void Awake()
    {
        CommandManager.OnExecute += OnUpdate;
        CommandManager.OnUndo += OnUpdate;
        CommandManager.OnRedo += OnUpdate;

        OnUpdate();
    }

    private void OnDestroy()
    {
        CommandManager.OnExecute -= OnUpdate;
        CommandManager.OnUndo -= OnUpdate;
        CommandManager.OnRedo -= OnUpdate;
    }

    public void Undo()
    {
        CommandManager.UndoAsync().Forget();
    }

    public void Redo()
    {
        CommandManager.RedoAsync().Forget();
    }

    private void OnUpdate()
    {
        onUndoStateChanged?.Invoke(CommandManager.CanUndo());
        onRedoStateChanged?.Invoke(CommandManager.CanRedo());
    }
}
