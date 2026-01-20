using System;
using AI.GOAP.Agent;
using Managers;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace AI.GOAP
{
    public interface IStrategies
    {
        public bool CanPerform { get; }
        public bool Complete { get; }

        public void Start()
        {
        }

        public void Update(float deltaTime);
        public void Stop();
    }
    
    public class IdleStrategy : IStrategies
    {
        public bool CanPerform => true;
        public bool Complete { get; private set; }

        private readonly CountdownTimer _timer;

        public IdleStrategy(float duration)
        {
            _timer = new CountdownTimer(duration);
            _timer.OnTimerStart += () => Complete = false;
            _timer.OnTimerStop += () => Complete = true;
        }

        public void Update(float deltaTime) => _timer.Tick(deltaTime);

        public void Stop() { }
    }
    public class WanderStrategy : IStrategies
    {
        private readonly NavMeshAgent _agent;
        private readonly float _wanderRadius;
        [SerializeField] private GoapAgentStats _stats;
        
        public bool CanPerform => !Complete;
        public bool Complete => _agent.remainingDistance <= 2f && !_agent.pathPending;

        public WanderStrategy(NavMeshAgent agent, float wanderRadius)
        {
            _agent = agent;
            _wanderRadius = wanderRadius;
        }

        public void Start()
        {
            for (int i = 0; i < _stats.Tries; i++)
            {
                Vector3 randDir = (UnityEngine.Random.insideUnitSphere * _wanderRadius).With(y:0);
                NavMeshHit hit;

                if (NavMesh.SamplePosition(_agent.transform.position + randDir, out hit, _wanderRadius, StaticUtilities.PlayerLayer))
                {
                    _agent.SetDestination(hit.position);
                    return;
                }
            }
        }
        
        public void Update(float deltaTime) { }
        public void Stop() { }
    }

    public class MoveStrategy : IStrategies
    {
        private readonly NavMeshAgent _agent;
        private readonly Func<Vector3> _destination;

        public bool CanPerform => !Complete;
        public bool Complete => _agent.remainingDistance <= 2f && !_agent.pathPending;

        public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination)
        {
            _agent = agent;
            _destination = destination;
        }
        
        public void Start() => _agent.SetDestination(_destination());
        public void Update(float deltaTime) { }
        public void Stop() => _agent.ResetPath();
    }
}
