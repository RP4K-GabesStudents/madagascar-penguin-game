using ObjectiveSystem.Core;
using UnityEngine;

namespace Utilities.Utilities.Common.Settings
{
    public class TestingDummy : MonoBehaviour, IKillTaskObservable
    {
        private void OnDestroy()
        {
            TaskObservableEventBus<TestingDummy>.Publish(EActionType.Kill);
        }
    }
}