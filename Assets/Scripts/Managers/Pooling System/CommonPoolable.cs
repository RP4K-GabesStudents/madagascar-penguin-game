using System;
using UnityEngine;

namespace Managers.Pooling_System
{
    public class CommonPoolable : MonoBehaviour, IPoolable
    {
        [SerializeField] private float lifeTime;
        private float _curTime;

        private void Update()
        {
            _curTime -= Time.deltaTime;
            if (_curTime <= 0)
            {
                gameObject.SetActive(false);
            }
        }
        
        private void OnEnable()
        {
            _curTime = lifeTime;
        }

        public void ForceDespawn()
        {
            
        }
        public void Spawn(ulong spawnID)
        {
            
        }

    }
}
