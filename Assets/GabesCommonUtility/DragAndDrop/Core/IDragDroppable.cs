
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DragAndDrop.Core
{
    /// <summary>
    /// Interface for objects that can be dragged and dropped
    /// </summary>
    public interface IDragDroppable
    {
        IDragDropZone currentTarget { get; }
        IDragDropZone oldTarget { get; }
        Transform transform { get; } // Added to allow movement
        void SetParent(IDragDropZone target);
        UniTask SetParentAsync(IDragDropZone target);
        int GetLayers();
    }
}