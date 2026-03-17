using ObjectiveSystem.Core;
using ObjectiveSystem.Task;
using UnityEngine;

namespace Utilities.Utilities.Common.Settings
{
    public class TestingTask : MonoBehaviour
    {
        private ITask _test1;
        private ITask _test2;
        TaskGroupTask _taskGroup;
        [SerializeField] private int testingAmount1 = 5;
        [SerializeField] private int testingAmount2 = 5;
        private void Awake()
        {
            
            _test1 = new KillTask<TestingDummy>(false, "test 1", testingAmount1);
            _test2 = new KillTask<TestingDummy2>(false, "test 2", testingAmount2);
            _taskGroup = new TaskGroupTask("Testing Group", false);
            _taskGroup.AddTask(_test1);
            _taskGroup.AddTask(_test2);
            
            _test1.OnComplete += () => Display(_test1);
            _test2.OnComplete += () => Display(_test2);
            _taskGroup.OnComplete += () => Display(_taskGroup);
        }

        private void Display(ITask t)
        {
            Debug.Log("Our testing tasking has been completed! " + t.TaskName);
            t.Dispose();
        }
    }
    
    
}