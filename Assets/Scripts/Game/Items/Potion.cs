using Game.Characters.World;
using Game.InventorySystem;
using Game.Objects;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// A potion. On the ground it is a WorldItem you can hover/pick up. Once held
/// (InventoryCapability parented it into the mouth) it is an IHeldItem. It also
/// breaks on hard impact and dies in one hit (IDamageable).
///
/// Lightweight damageable: it has no persistent networked health (it dies in
/// one hit and despawns, and Despawn already replicates), so it carries only
/// the one required IDamageable stub (TakeDamageRpc) and nothing more.
///
/// Breaking is wired over the network: the server decides it broke, then fans
/// the visual out to everyone via an Rpc.
/// </summary>
public class Potion : WorldItem, IHeldItem, IDamageable
{
    [SerializeField] private float breakForce = -9f; // square manually: 10 -> 100

    private float _previousSpeed;
    private bool _broken;

    // IDamageable: every potion dies in one hit. Health is a constant; the hit
    // breaks it via OnHurt regardless of the (no-op) Health subtraction.
    public float Health { get => 1; set { } }
    public float DamageRes => 0;

    // The one required per-class IDamageable stub. Verbatim across implementers;
    // must live on the concrete NetworkBehaviour because [Rpc] needs one.
    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(float damage, Vector3 force) =>
        ((IDamageable)this).TakeDamageLocal(damage, force);

    private void FixedUpdate()
    {
        if (_rb) _previousSpeed = _rb.linearVelocity.sqrMagnitude;
    }

    private void OnCollisionEnter(Collision _)
    {
        if (!IsServer || _rb == null) return; // server decides breakage

        Vector3 velocity = _rb.linearVelocity;
        float speed = velocity.sqrMagnitude;

        if (speed - _previousSpeed < breakForce)
            Break(velocity);
    }

    public void Die(Vector3 force) => Break(force.normalized);

    public void OnHurt(float amount, Vector3 force) => Break(-force);

    // ---- IHeldItem ----

    public void OnEquip(GenericCharacter owner) { }

    public void OnStartUse()
    {
        // Drinking / throwing logic goes here. Runs server-side (the use
        // lifecycle is invoked on the server).
    }

    public void OnStopUse() { }

    public void OnUnequip() { }

    // ---- Breaking ----

    private void Break(Vector3 direction)
    {
        if (!IsServer || _broken) return;
        _broken = true;

        Break_Rpc(direction);

        // Server owns lifetime: after the visual plays, the object is gone.
        if (IsSpawned) NetworkObject.Despawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void Break_Rpc(Vector3 direction)
    {
        // TODO(team): spawn shatter VFX / play sound / splash effect at
        // transform.position oriented along 'direction'. Runs on every client.
        Debug.Log("Potion broke", gameObject);
    }
}