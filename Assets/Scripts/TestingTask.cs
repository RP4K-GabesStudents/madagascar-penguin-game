using ObjectiveSystem.Core;
using ObjectiveSystem.Task;
using UnityEngine;

namespace Utilities.Utilities.Common.Settings
{
    public class TestingTask : MonoBehaviour
    {
        private ITask _test1;
        private ITask _test2;
        [SerializeField] private int testingAmount1 = 5;
        [SerializeField] private int testingAmount2 = 5;
        private void Awake()
        {
            _test1 = new KillTask<TestingDummy>(false, "test", testingAmount1);
            _test2 = new KillTask<TestingDummy2>(false, "test", testingAmount2);
            _test1.OnComplete += () => Display(_test1);
            _test2.OnComplete += () => Display(_test2);
        }

        private void Display(ITask t)
        {
            Debug.Log("Our testing tasking has been completed! " + t.TaskName);
            t.Dispose();
        }
    }
    
    
}