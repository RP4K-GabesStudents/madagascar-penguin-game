using System;
using Scriptable_Objects;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[SelectionBase]
public class PlayerController : MonoBehaviour
{
    [SerializeField]PenguinStats penguinStats;
    private Rigidbody _rigidbody;
    private Vector3 _curMoveDir;
    private Vector2 _curLookDir;
    [Header("Jumping")] 
    [SerializeField] private float footDist;
    [SerializeField] private float footRadius;
    [SerializeField] private Transform foot;
    
    [Header("Animation")]
    [SerializeField] private Transform headProxy;
    [SerializeField] private Transform headBone;
    [SerializeField, Min(0)] private float rotationAnimationSpeed;
    [SerializeField, Min(0)] private float animationReturnSpeed;

    [SerializeField, Range(-1,1)] private float rotationAnimationThreshold;

    
    [Header("Looking")] 
    [SerializeField] private Transform headXRotator;
    [SerializeField] private Transform bodyYRotator;
    [SerializeField, Min(0)] private float rotationSpeed;
    [SerializeField, Range(0,90)] private float pitchLimit;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        _rigidbody.AddForce(transform.rotation * _curMoveDir * penguinStats.Speed);
    }

    private void LateUpdate()
    {
        HandleLooking();
        /*
        float dot = Vector3.Dot(head.forward, lookPivot.forward);
        if (dot > rotationThreshold)
        {
            head.Rotate(Vector3.right, _curLookDir.y * Time.deltaTime * 100);
            body.Rotate(Vector3.up, _curLookDir.x * Time.deltaTime * 100);
        }
        else
        {
            lookPivot.Rotate(Vector3.right, _curLookDir.y * Time.deltaTime * 100);
            lookPivot.Rotate(Vector3.up, _curLookDir.x * Time.deltaTime * 100);
            if (lookPivot.eulerAngles.y > pitchLimit)
            {
                lookPivot.eulerAngles = new Vector3(lookPivot.eulerAngles.x, pitchLimit, lookPivot.eulerAngles.z);
            }
            else if (lookPivot.eulerAngles.y < -pitchLimit)
            {
                lookPivot.eulerAngles = new Vector3(lookPivot.eulerAngles.x, -pitchLimit, lookPivot.eulerAngles.z);
            }
        }
        theRealHead.rotation = lookPivot.rotation;
        */
    }

    private void HandleLooking()
    {
        float dt = Time.deltaTime; //Cache this for easy optimization
        
        //First, let's rotate around the Y axis (left and right rotation)
        bodyYRotator.Rotate(Vector3.up, _curLookDir.x * rotationSpeed * dt);
        
        //Next, we want to rotate around the X axis, BUT we need to be careful to not look to high up.
        float newXRotation = headXRotator.localEulerAngles.x;
        
        newXRotation = (newXRotation > 180f) ? newXRotation - 360f : newXRotation; // Convert to -180 to 180 range (Because Unity sucks)
        
        newXRotation = Mathf.Clamp(newXRotation + _curLookDir.y * rotationSpeed * dt, -pitchLimit, pitchLimit);
        
        //Set the rotation
        headXRotator.localRotation = Quaternion.Euler(newXRotation, headXRotator.localEulerAngles.y, headXRotator.localEulerAngles.z);
        
       //Rotate the head in both directions...
       headProxy.Rotate(Vector3.up, _curLookDir.x * rotationAnimationSpeed * dt);
       headProxy.Rotate(Vector3.right, _curLookDir.y * rotationAnimationSpeed * dt);

       //Check if we exceed the threshold, if we do then rotate the head slowly to correct location
       if (Vector3.Dot(headProxy.forward, headXRotator.forward) < rotationAnimationThreshold)
       {
           headProxy.rotation= Quaternion.Slerp(headProxy.rotation, headXRotator.rotation, animationReturnSpeed * dt); // Use rotation speed
       }
       
       headBone.rotation = headProxy.rotation;
        
    }

    public void Jump(bool readValueAsButton)
    {
        
    }

    public void Attack(bool readValueAsButton)
    {
        
    }   

    public void Sprint(bool readValueAsButton)
    {
        
    }

    public void Crouch(bool readValueAsButton)
    {
        
    }

    public void Interact(bool readValueAsButton)
    {
        
    }

    public void SetMoveDirection(Vector3 moveDirection)
    {
        _curMoveDir = moveDirection;
    }

    public void Look(Vector2 lookDirection)
    {
        _curLookDir = lookDirection;
    }
}
