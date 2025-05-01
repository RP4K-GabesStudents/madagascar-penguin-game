using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Horse_Foler
{
    public class Horse : NetworkBehaviour
    {
        [SerializeField] private Transform target;
        private NavMeshAgent _navMesh;
        

        private void Awake()
        {
            _navMesh = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            _navMesh.SetDestination(target.position);
        }
    }
}
