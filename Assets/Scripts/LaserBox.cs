using System;
using Scriptable_Objects;
using Scriptable_Objects.Penguin_Stats;
using UnityEngine;
using UnityEngine.Serialization;

public class LaserBox : MonoBehaviour
{
    [SerializeField]private PenguinStats penguinStats;
    private void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
        penguinStats.CanShootLaser = true;
    }
}
