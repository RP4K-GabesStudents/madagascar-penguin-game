using System;
using Scriptable_Objects;
using UnityEngine;
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
    [Header("Looking")] 
    [SerializeField] private Transform lookPivot;
    [SerializeField] private Transform theRealHead;
    [SerializeField] private Transform head;
    [SerializeField] private Transform body;
    [SerializeField] private float rotationThreshold;
    [SerializeField] private float pitchLimit;
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
