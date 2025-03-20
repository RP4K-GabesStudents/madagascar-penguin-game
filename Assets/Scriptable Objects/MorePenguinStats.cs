using UnityEngine;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "MorePenguinStats", menuName = "Scriptable Objects/MorePenguinStats")]
    public class MorePenguinStats : ScriptableObject
    {
        [SerializeField] private float interactRadius;
        [SerializeField] private float interactDistance;
    }
}
