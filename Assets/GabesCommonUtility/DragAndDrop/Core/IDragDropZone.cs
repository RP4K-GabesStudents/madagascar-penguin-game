
using UnityEngine;

namespace DragAndDrop.Core
{
    /// <summary>
    /// Interface for zones that can accept dragged objects
    /// </summary>
    public interface IDragDropZone
    {
        Transform transform { get; } // Added for parenting
        bool CanAcceptItem(IDragDroppable obj);
        void OnItemGained(IDragDroppable obj);
        void OnItemLost(IDragDroppable obj);
        Vector2 GetNearestAnchoredPosition(Vector2 currentPosition);
    }
    
}