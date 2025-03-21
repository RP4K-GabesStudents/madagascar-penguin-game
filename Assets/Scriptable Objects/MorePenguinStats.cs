using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "MorePenguinStats", menuName = "Scriptable Objects/MorePenguinStats")]
    public class MorePenguinStats : ScriptableObject
    {
        [SerializeField] private float interactRadius;
        [SerializeField] private float interactDistance;
        [SerializeField] private LayerMask interactLayer;
        
        public float InteractRadius => interactRadius;
        public float InteractDistance => interactDistance;
        public LayerMask InteractLayer => interactLayer;
    }
}
