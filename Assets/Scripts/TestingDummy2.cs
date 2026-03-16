using ObjectiveSystem.Core;
using UnityEngine;

namespace Utilities.Utilities.Common.Settings
{
    public class TestingDummy2 : MonoBehaviour, IKillTaskObservable, ISpottedObservable
    {
        private void OnDestroy()
        {
            TaskObservableEventBus<TestingDummy2>.Publish(EActionType.Kill);
        }


        private void Start()
        {
            TaskObservableEventBus<TestingDummy>.Publish(EActionType.Spotted);
        }
    }
}