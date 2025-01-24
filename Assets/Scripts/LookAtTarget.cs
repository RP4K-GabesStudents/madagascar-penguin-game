using UnityEngine;

public class LookAtMouse : MonoBehaviour
{
    [SerializeField] private Transform headBone;
    [SerializeField] private Transform target;

    // Update is called once per frame
    void LateUpdate()
    {
        headBone.LookAt(target);
    }
}
