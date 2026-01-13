using UnityEngine;

namespace Detection.Core
{
    public interface IDetectable
    {
        public void OnDetectedBy(MonoBehaviour detector);
        public void OnDetectionLost(MonoBehaviour detector);
    }
}