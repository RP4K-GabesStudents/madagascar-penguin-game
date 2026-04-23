using ObjectiveSystem.Core;
using ObjectiveSystem.Task;
using UnityEngine;

namespace Utilities.Utilities.Common.Settings
{
    public class TestingTask : MonoBehaviour
    {
        private ITaskConditional _test1;
        private ITaskConditional _test2;
        [SerializeField] private int testingAmount1 = 5;
        [SerializeField] private int testingAmount2 = 5;
        private void Awake()
        {
            
            _test1 = new KillConditional<TestingDummy>(false, "test 1", testingAmount1);
            _test2 = new KillConditional<TestingDummy2>(false, "test 2", testingAmount2);
            
            /*
            _taskConditionalGroup = new TaskConditionalGroupTaskConditional("Testing Group", false);
            _taskConditionalGroup.AddTask(_test1);
            _taskConditionalGroup.AddTask(_test2);
            */
            
            _test1.OnComplete += () => Display(_test1);
            _test2.OnComplete += () => Display(_test2);
        }

        private void Display(ITaskConditional t)
        {
            Debug.Log("Our testing tasking has been completed! " + t.TaskName);
            t.Dispose();
        }
    }
    
    
}