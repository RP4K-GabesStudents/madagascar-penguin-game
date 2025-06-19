using System;
using Interfaces;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

[SelectionBase]
public class Potion : NetworkBehaviour, IDamageable, IInteractable
{
    private float _previousSpeed;
    [SerializeField] private HoverInfoStats hoverInfo;
    [SerializeField] private float breakForce = -9; // This number should be squared manually so 10 --> 100.
    private Rigidbody _rb;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        _previousSpeed = _rb.linearVelocity.sqrMagnitude;
    }

    private void OnCollisionEnter(Collision other)
    {
        //While using SqrMagnitude is logically faster, the difference is very minor.
        Vector3 velocity = _rb.linearVelocity;
        float speed = velocity.sqrMagnitude;
            
        //If it's negative, then we've suddenly slowed down
        if (speed - _previousSpeed < breakForce)
        {
            Break(velocity);
        }
    }
    public void Die()
    {
        Destroy(gameObject);
    }

    public void OnHurt(float amount, Vector3 force)
    {
        Break(-force);
    }

    private void Break(Vector3 direction)
    {
        //var c = Instantiate(cloud, transform.position, Quaternion.LookRotation(direction));
        //c.Initialize(_effect);
        //Die();
    }
    public float Health { get; set; }
    public float DamageRes { get; }

    

    public void OnHover()
    {
       Debug.Log("potion hover");
    }

    public void OnHoverEnd()
    {
        Debug.Log("potion hover end");
    }

    public void OnInteract()
    {
        Debug.Log("potion hover interact");
    }

    public HoverInfoStats GetHoverInfoStats()
    {
        return hoverInfo;
    }

    public bool CanHover()
    {
        return true;
    }
}