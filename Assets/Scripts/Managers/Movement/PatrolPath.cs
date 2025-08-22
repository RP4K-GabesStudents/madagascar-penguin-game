using UnityEngine;

namespace Managers.Movement
{
    public class PatrolPath : MonoBehaviour
    {
        [SerializeField] private PatrolPathSoap patrolPathSoap;

        private void Awake()
        {
            patrolPathSoap.waypoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                patrolPathSoap.waypoints[i] = transform.GetChild(i);
            }
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(transform.GetChild(i).position, 0.1f);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild((i + 1) % transform.childCount).position);
            }
        }
    }
    
}
