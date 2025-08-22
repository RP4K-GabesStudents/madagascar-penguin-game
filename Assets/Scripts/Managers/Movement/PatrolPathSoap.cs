using UnityEngine;

namespace Managers.Movement
{
    [CreateAssetMenu(fileName = "PatrolPathSoap", menuName = "SOAP/PatrolPathSoap")]

    public class PatrolPathSoap : ScriptableObject
    {
        public Transform[] waypoints;
    }
}
