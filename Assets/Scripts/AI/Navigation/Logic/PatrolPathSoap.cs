using UnityEngine;

namespace AI.Navigation.Logic
{
    [CreateAssetMenu(fileName = "PatrolPathSoap", menuName = "SOAP/PatrolPathSoap")]

    public class PatrolPathSoap : ScriptableObject
    {
        public Transform[] waypoints;
    }
}
