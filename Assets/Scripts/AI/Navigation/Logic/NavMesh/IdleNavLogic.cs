using System.Collections;
using AI.Navigation.Core;
using UnityEngine;

namespace AI.Navigation.Logic.NavMesh

{
    public class IdleNavLogic : BaseNavMeshNavigation, INavigationMode
    {
        [SerializeField] private float minIdleTime;
        [SerializeField] private float maxIdleTime;
        
        public float GetRandIdleTime() => Random.Range(minIdleTime, maxIdleTime);
        
        public IEnumerator ExecuteState()
        {
            Agent.SetDestination(Agent.nextPosition);
            yield return new WaitForSeconds(GetRandIdleTime());
        }
    }
}
