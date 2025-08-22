using System;
using UnityEngine;

namespace Managers.Movement.Stats
{
    public abstract class MoveTypeStats : ScriptableObject
    {
        public abstract Type CreateTypeObject();
    }
}
