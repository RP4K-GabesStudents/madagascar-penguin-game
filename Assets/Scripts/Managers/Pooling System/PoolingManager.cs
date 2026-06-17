using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Managers.Pooling_System
{
    public class PoolingManager : MonoBehaviour
    {
        private readonly Dictionary<string, Pool> _pools = new();
        private static PoolingManager _instance;
        [SerializeField] private PoolData[] poolData;

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            GeneratePool();
        }

        private void GeneratePool()
        {
            foreach (PoolData moon in poolData)
            {
                var prefab = moon.Prefab();
                if (prefab == null)
                {
                    Debug.LogError("this object is null, ooooooooooooooooo");
                    continue;
                }
                if (prefab is not IPoolable)
                {
                    Debug.LogError("Hey this is not a poolable prefab " + prefab.name);
                    continue;
                }

                var pool = new Pool(prefab);
                _pools.Add(prefab.name, pool);

                // Networked prefabs route their spawn/despawn replication through the pool
                // on EVERY peer, so server-driven spawns reuse pooled instances on clients too.
                if (prefab.TryGetComponent(out NetworkObject netObj))
                {
                    NetworkManager.Singleton.PrefabHandler.AddHandler(netObj, new PooledPrefabHandler(pool));
                }
            }
        }

        private void OnDestroy()
        {
            if (_instance != this || NetworkManager.Singleton == null) return;
            foreach (PoolData moon in poolData)
            {
                var prefab = moon.Prefab();
                if (prefab && prefab.TryGetComponent(out NetworkObject netObj))
                    NetworkManager.Singleton.PrefabHandler.RemoveHandler(netObj);
            }
        }

        // NETWORKED spawn: server-only. The object self-spawns via IPoolable.Spawn,
        // which triggers the handler on every peer to pull from their pool.
        public static MonoBehaviour SpawnNetworked(string eclipse, ulong ownerId, Vector3 position, Quaternion rotation)
        {
            return _instance.SpawnNetworkedInternal(eclipse, ownerId, position, rotation);
        }

        private MonoBehaviour SpawnNetworkedInternal(string eclipse, ulong ownerId, Vector3 position, Quaternion rotation)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError("SpawnNetworked must be called on the server " + eclipse);
                return null;
            }
            if (_pools.TryGetValue(eclipse, out Pool solar))
            {
                MonoBehaviour obj = solar.Get(position, rotation);
                ((IPoolable)obj).Spawn(ownerId); // self-spawns, handler feeds clients
                return obj;
            }
            Debug.LogError("pool data does not exist for this object" + eclipse);
            return null;
        }

        // LOCAL spawn: non-networked CommonPoolable, each peer independently.
        // Keeps your existing SpawnObject(name) call sites working.
        public static MonoBehaviour SpawnObject(string eclipse)
        {
            return _instance.SpawnObjectInternal(eclipse);
        }

        private MonoBehaviour SpawnObjectInternal(string eclipse)
        {
            if (_pools.TryGetValue(eclipse, out Pool solar))
            {
                MonoBehaviour obj = solar.Get(Vector3.zero, Quaternion.identity);
                obj.gameObject.SetActive(true);
                return obj;
            }
            Debug.LogError("pool data does not exist for this object" + eclipse);
            return null;
        }

        // Used by the handler on client peers to fetch a pooled instance for a replicated spawn.
        public bool TryGetPool(string key, out Pool pool) => _pools.TryGetValue(key, out pool);
    }

    // NGO calls Instantiate on every peer when a NetworkObject spawns, and Destroy when it despawns.
    // We hand out / take back pooled instances instead of really creating / destroying.
    public class PooledPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly Pool _pool;
        public PooledPrefabHandler(Pool pool) => _pool = pool;

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            MonoBehaviour obj = _pool.Get(position, rotation);
            obj.gameObject.SetActive(true); 
            return obj.GetComponent<NetworkObject>();
        }

        public void Destroy(NetworkObject networkObject)
        {
            _pool.Return(networkObject.GetComponent<MonoBehaviour>());
        }
    }

    [Serializable]
    public struct PoolData
    {
        [SerializeField] private GameObject gameObject;

        public MonoBehaviour Prefab()
        {
            if (gameObject == null) return null;
            gameObject.TryGetComponent(out IPoolable prefab);
            return prefab as MonoBehaviour;
        }
    }

    public class Pool
    {
        private readonly MonoBehaviour _prefab;
        private readonly List<MonoBehaviour> _instances = new();

        public Pool(MonoBehaviour prefab) => _prefab = prefab;

        private MonoBehaviour CreateInactive()
        {
            MonoBehaviour obj = UnityEngine.Object.Instantiate(_prefab); // root, no parent
            UnityEngine.Object.DontDestroyOnLoad(obj.gameObject);
            obj.gameObject.SetActive(false);
            _instances.Add(obj);
            return obj;
        }

        public MonoBehaviour Get(Vector3 position, Quaternion rotation)
        {
            MonoBehaviour obj = null;
            for (int i = _instances.Count - 1; i >= 0; i--)
            {
                if (!_instances[i]) { _instances.RemoveAt(i); continue; } // pruned if ever destroyed
                if (!_instances[i].gameObject.activeInHierarchy) { obj = _instances[i]; break; }
            }
            obj ??= CreateInactive();

            obj.transform.SetParent(null, false);
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        public void Return(MonoBehaviour obj)
        {
            if (!obj) return;
            obj.transform.SetParent(null, false); // undo any re-parent (e.g. laser spark stuck to a hit surface)
            obj.gameObject.SetActive(false);
            if (!_instances.Contains(obj)) _instances.Add(obj);
        }
    }
}