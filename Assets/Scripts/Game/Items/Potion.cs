using Game.InventorySystem;
using Game.Objects;
using Scriptable_Objects;
using UnityEngine;


public class Potion : Item, IDamageable
{
    [SerializeField] private HoverInfoStats hoverInfo;
    [SerializeField] private float breakForce = -9; // This number should be squared manually so 10 --> 100.
    
    private Rigidbody _rb;
    private float _previousSpeed;
    
    
    //part of IDamagable, all potions should break in 1 hit.
    public float Health
    {
        get => 1;
        set { }
    }
    public float DamageRes => 0;
    

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        //Store the previous speed every tick
        _previousSpeed = _rb.linearVelocity.sqrMagnitude;
    }

    private void OnCollisionEnter(Collision _)
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
    public void Die(Vector3 force)
    {
        Break(force.normalized);
    }

    public void OnHurt(float amount, Vector3 force)
    {
        Break(-force);
    }

    private void Break(Vector3 direction)
    {
        Debug.LogError("Hey Mr. Austin, please implement this function! NOTE: This will not work over netcode by default, I want you to try figure it out using RPCs");
    }
}