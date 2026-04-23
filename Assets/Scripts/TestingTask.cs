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
                objective.OnUpdate += UpdateTask; //<< When the task is done...
            }
        }
    }
    
    
}