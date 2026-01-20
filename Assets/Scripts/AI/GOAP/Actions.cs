using System;
using System.Collections.Generic;
using AI.GOAP.Agent;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace AI.GOAP
{
    public class Actions : IStrategies
    {
        public string Name { get; }
        public float Cost { get; private set; }
        public HashSet<AgentBelief> PreConditions { get; } = new ();
        public HashSet<AgentBelief> Effects { get; } = new();
        public bool CanPerform { get; }
        public bool Complete { get; }
        private IStrategies _strategy;

        private Actions(string name)
        {
            Name = name;
        }

        public void Start() => _strategy.Start();
            
        

        public void Update(float deltaTime)
        {
            if (CanPerform)
            {
                Update(deltaTime);
            }

            if (!Complete) return;

            foreach (var effect in Effects)
            {
                effect.Evaluate();
            }
        }
        public void Stop() { }
        public class Builder
        {
            private readonly Actions _action;

            public Builder(string name)
            {
                _action = new Actions(name){ Cost = 1 };
            }

            public Builder WithCost(float cost)
            {
                _action.Cost = cost;
                return this;
            }

            public Builder WithStrategy(IStrategies strategy)
            {
                _action._strategy = strategy;
                return this;
            }

            public Builder AddPrecondition(AgentBelief condition)
            {
                _action.PreConditions.Add(condition);
                return this;
            }

            public Builder AddEffect(AgentBelief effect)
            {
                _action.Effects.Add(effect);
                return this;
            }

            public Actions Build()
            {
                return _action;
            }
        }
    }
}
