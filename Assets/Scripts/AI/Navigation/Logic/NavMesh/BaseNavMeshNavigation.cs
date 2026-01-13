using UnityEngine;
using UnityEngine.AI;

namespace AI.Navigation.Logic.NavMesh
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BaseNavMeshNavigation : MonoBehaviour
    {
        private NavMeshAgent _agent;

        protected NavMeshAgent Agent => _agent ??= GetComponent<NavMeshAgent>();
    }
}