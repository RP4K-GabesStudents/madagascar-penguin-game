using UnityEngine;

namespace Utilities
{
    [RequireComponent(typeof(Canvas))]
    public class WorldCameraAutoAttach : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponent<Canvas>().worldCamera = Camera.main;
        }
    }
}
