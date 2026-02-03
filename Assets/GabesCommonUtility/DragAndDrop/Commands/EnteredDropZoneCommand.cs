using System;
using System.Threading;
using Commands.Core;
using Cysharp.Threading.Tasks;
using DragAndDrop.Core;

namespace DragAndDrop.Commands
{
    /// <summary>
    /// Command for handling drag-and-drop operations with undo/redo support
    /// </summary>
    public class EnteredDropZoneCommand : ICommand
    {
        private readonly IDragDroppable _dragDroppable;
        private readonly IDragDropZone _previousZone;
        private readonly IDragDropZone _newZone;
        private readonly Func<UniTask> _onExecute;
        private readonly Func<UniTask> _onUndo;
        private readonly bool _waitForCompletion;

        public string DisplayName => $"Move to {_newZone?.GetType().Name ?? "Zone"}";
        
        /// <summary>
        /// Creates a command with default move behavior
        /// </summary>
        public EnteredDropZoneCommand(
            IDragDroppable dragDroppable, 
            IDragDropZone previousZone, 
            IDragDropZone newZone)
        {
            _dragDroppable = dragDroppable ?? throw new ArgumentNullException(nameof(dragDroppable));
            _previousZone = previousZone;
            _newZone = newZone ?? throw new ArgumentNullException(nameof(newZone));
            _waitForCompletion = false;
        }
        
        /// <summary>
        /// Creates a command with custom execute action
        /// </summary>
        public EnteredDropZoneCommand(
            IDragDroppable dragDroppable, 
            IDragDropZone previousZone, 
            IDragDropZone newZone, 
            Func<UniTask> onExecute,
            bool waitForCompletion = false)
        {
            _dragDroppable = dragDroppable ?? throw new ArgumentNullException(nameof(dragDroppable));
            _previousZone = previousZone;
            _newZone = newZone ?? throw new ArgumentNullException(nameof(newZone));
            _onExecute = onExecute;
            _waitForCompletion = waitForCompletion;
        }

        /// <summary>
        /// Creates a command with custom execute and undo actions
        /// </summary>
        public EnteredDropZoneCommand(
            IDragDroppable dragDroppable, 
            IDragDropZone previousZone, 
            IDragDropZone newZone, 
            Func<UniTask> onExecute,
            Func<UniTask> onUndo,
            bool waitForCompletion = false)
        {
            _dragDroppable = dragDroppable ?? throw new ArgumentNullException(nameof(dragDroppable));
            _previousZone = previousZone;
            _newZone = newZone ?? throw new ArgumentNullException(nameof(newZone));
            _onExecute = onExecute;
            _onUndo = onUndo;
            _waitForCompletion = waitForCompletion;
        }

        public async UniTask ExecuteAsync(CancellationToken ct = default)
        {
            if (_onExecute != null)
            {
                if (_waitForCompletion)
                {
                    await _onExecute.Invoke();
                }
                else
                {
                    _onExecute.Invoke().Forget();
                }
            }
            else
            {
                // Default behavior: move to new zone
                //await _dragDroppable.SetParentAsync((IDragAndDropZone)_newZone);
            }
        }

        public async UniTask UndoAsync(CancellationToken ct = default)
        {
            if (_onUndo != null)
            {
                if (_waitForCompletion)
                {
                    await _onUndo.Invoke();
                }
                else
                {
                    _onUndo.Invoke().Forget();
                }
            }
            else if (_previousZone != null)
            {
                // Default behavior: return to previous zone
                //await _dragDroppable.SetParentAsync((DragAndDropZone)_previousZone);
            }
        }
    }
}