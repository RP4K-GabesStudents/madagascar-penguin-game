using System.Collections.Generic;
using AI.GOAP.Agent;
using UnityEngine;

namespace AI.GOAP
{
    public class Goals
    {
        public string name {get; }
        public float priority {get; private set; }
        public HashSet<AgentBelief> DesiredEffects { get; } = new();

        private Goals(string name)
        {
            this.name = name;
        }

        public class Builder
        {
            private readonly Goals _goals;
            public Builder(string name)
            {
                _goals = new Goals(name);
            }

            public Builder WithPriority(float priority)
            {
                _goals.priority = priority;
                return this;
            }

            public Builder WithDesiredEffects(AgentBelief desiredEffect)
            {
                _goals.DesiredEffects.Add(desiredEffect);
                return this;
            }

            public Goals Build()
            {
                return _goals;
            }
        }
    }
}
