
using System;
using System.Collections.Generic;
using System.Linq;
using AI.GOAP.Agent;
using UnityEngine;
using UnityEngine.Animations;

namespace AI.GOAP
{
    public interface IGoapPlanner
    {
        ActionPlan Plan(GoapAgent agent, HashSet<Goals> goals, Goals mostRecentGoal = null);
    }

    public class GoapPlanner : IGoapPlanner
    {
        public ActionPlan Plan(GoapAgent agent, HashSet<Goals> goals, Goals mostRecentGoal = null)
        {
            List<Goals> orderedGoals = goals.Where(g => g.DesiredEffects.Any(b => !b.Evaluate())).OrderByDescending(g => g == mostRecentGoal ? g.priority - 0.01 : g.priority).ToList();

            //try to solve each goal in order
            foreach (var goal in orderedGoals)
            {
                Node goalNode = new Node(null, null, goal.DesiredEffects, 0);

                if (FindPath(goalNode, agent.Actions))
                {
                    //if the goalnode has no leaves and no action to perform try a different goal
                    if (goalNode.IsLeafDead) continue;

                    Stack<Actions> actionsStack = new Stack<Actions>();
                    while (goalNode.Leaves.Count > 0)
                    {
                        var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                        goalNode = cheapestLeaf;
                        actionsStack.Push(cheapestLeaf.Actions);
                    }

                    return new ActionPlan(goal, actionsStack, goalNode.Cost);
                }
            }
            Debug.LogWarning("No Plan Found");
            return null;
        }

        private bool FindPath(Node parent, HashSet<Actions> actions)
        {
            foreach (var action in actions)
            {
                var requiredEffects = parent.RequiredEffects;

                //remove any effects that are true but have no actions
                requiredEffects.RemoveWhere(b => b.Evaluate());

                // if there are no required effects to fulfill we have a plan 
                if (requiredEffects.Count == 0)
                {
                    return true;
                }

                if (action.Effects.Any(requiredEffects.Contains))
                {
                    var newRequiredEffects = new HashSet<AgentBelief>(requiredEffects);
                    newRequiredEffects.ExceptWith(action.Effects);
                    newRequiredEffects.UnionWith(action.PreConditions);

                    var newAvailableActions = new HashSet<Actions>(actions);
                    newAvailableActions.Remove(action);
                    
                    var newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.Cost);

                    if (FindPath(newNode, newAvailableActions))
                    {
                        parent.Leaves.Add(newNode);
                        newRequiredEffects.ExceptWith(newNode.Actions.PreConditions);
                    }

                    if (newRequiredEffects.Count == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class Node
    {
        public Node Parent { get; }
        public Actions Actions { get; }
        public HashSet<AgentBelief> RequiredEffects { get; }
        public List<Node> Leaves { get; }
        public float Cost { get; }
        public bool IsLeafDead => Leaves.Count == 0 && Actions == null;

        public Node(Node parent, Actions actions, HashSet<AgentBelief> requiredEffects, float cost)
        {
            Parent = parent;
            Actions = actions;
            RequiredEffects = new HashSet<AgentBelief>(requiredEffects);
            Leaves = new List<Node>();
            Cost = cost;
        }
    }

    public class ActionPlan
    {
        public Goals Goal { get; }
        public Stack<Actions> Actions { get; }
        public float TotalCost { get; set; }

        public ActionPlan(Goals goal, Stack<Actions> actions, float totalCost)
        {
            Goal = goal;
            Actions = actions;
            TotalCost = totalCost;
        }
    }
}
