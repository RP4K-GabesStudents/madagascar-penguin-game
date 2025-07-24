using System;
using UnityEngine;

namespace Game.Characters
{
    public abstract class CapabilityStats : ScriptableObject
    {
        [field: SerializeField] public bool DisplayOnUI { get; private set; } = false; // False by default.
        //This helps us figure out what object we want to create.
       // public abstract Type GetCapabilityType();
    }
}