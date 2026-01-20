using System;
using System.Collections.Generic;
using AI.GOAP.Sensor;
using UnityEngine;

namespace AI.GOAP.Agent
{
    //helps create dictionary with beliefs
    public class BeliefFactory
    {
        private readonly GoapAgent _goapAgent;
        private readonly Dictionary<string, AgentBelief> _beliefs;

        public BeliefFactory(GoapAgent goapAgent, Dictionary<string, AgentBelief> beliefs)
        {
            _goapAgent = goapAgent;
            _beliefs = beliefs;
        }

        public void AddBelief(string key, Func<bool> condition)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key).WithCondition(condition).Build());
        }

        public void AddSensorBelief(string key, Sensors sensor)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key).WithCondition(() => sensor.IsTargetInRange).WithLocation(() => sensor.TargetPosition).Build());
        }

        //replaces vector3 with transform
        public void AddLocationBelief(string key, float distance, Transform locationCondition)
        {
            AddLocationBelief(key, distance, locationCondition.position);
        }
        public void AddLocationBelief(string key, float distance, Vector3 locationCondition)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key).WithCondition(() => InRangeOf(locationCondition, distance)).WithLocation(() => locationCondition).Build());
        }
        
        private bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(_goapAgent.transform.position, pos) <= range;
    }

    public class AgentBelief
    {
        public string Name { get; }

        private Func<bool> _condition = () => false;
        private Func<Vector3> _observedLocation = () => Vector3.zero;
    
        public Vector3 Location => _observedLocation();

        AgentBelief(string name)
        {
            Name = name;
        }
        
        public bool Evaluate() => _condition();

        public class Builder
        {
            private readonly AgentBelief _agentBelief;

            public Builder(string name)
            {
                _agentBelief = new AgentBelief(name);
            }

            public Builder WithCondition(Func<bool> condition)
            {
                _agentBelief._condition = condition;
                return this;
            }

            public Builder WithLocation(Func<Vector3> location)
            {
                _agentBelief._observedLocation = location;
                return this;
            }

            public AgentBelief Build()
            {
                return _agentBelief;
            }
        }
    }
}
