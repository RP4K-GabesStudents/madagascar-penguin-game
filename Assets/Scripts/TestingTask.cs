using System;
using ObjectiveSystem.ScriptableObjects;
using UnityEngine;

namespace Utilities.Utilities.Common.Settings
{
    public class TestingTask : MonoBehaviour
    {
        [SerializeField] private Objective[] objectives;
        private void Awake()
        {
            foreach (Objective objective in  objectives)
            {
                objective.OnUpdate += () => UpdateTask(objective); //<< When the task is done...

                objective.BuildConditionals(); //<< Put this when we want to start... Bind this to an object too for timing... (Starts the mission)
                
                UpdateTask(objective);
            }
        }

        private void LateUpdate()
        {
            //This should be done in task manager... "Register Task"

            foreach (Objective objective in objectives)
                objective.Update();

        }

        private void UpdateTask(Objective objective)
        {
            if (objective.CurrentStatus != Objective.EObjectiveStatus.Active)
            {
                Debug.Log(objective.CurrentStatus);
                return;
            }
            
            objective.RebuildTextGame();
            Debug.Log(objective.CurrentText);
        }
    }
    
    
}