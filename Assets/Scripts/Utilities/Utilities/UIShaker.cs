using UnityEngine;

namespace Utilities
{
    public class UIShaker : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private float maxAngle;
        
        private Vector3 _initialRotation;

        // Update is called once per frame
        void LateUpdate()
        {
            transform.localEulerAngles += new Vector3(0,0,Time.deltaTime * speed);
            float angles = transform.localEulerAngles.z;
            if(angles > maxAngle && angles < 180) speed = -Mathf.Abs(speed);
            else if(angles < 360-maxAngle && angles > 180) speed = Mathf.Abs(speed);
        }

        private void OnEnable()
        {
            _initialRotation = transform.localEulerAngles;
        }

        private void OnDisable()
        {
            transform.localEulerAngles = _initialRotation;
        }
    }
}
