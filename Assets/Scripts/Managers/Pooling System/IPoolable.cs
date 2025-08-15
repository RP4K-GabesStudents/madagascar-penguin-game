
using UnityEngine;

namespace Managers.Pooling_System
{
    public interface IPoolable
    {
        public void Spawn(ulong spawnID);

        public void ForceDespawn();
    }
}
