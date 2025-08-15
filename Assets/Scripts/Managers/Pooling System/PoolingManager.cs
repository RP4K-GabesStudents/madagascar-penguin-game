using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers.Pooling_System
{
    public class PoolingManager : MonoBehaviour
    {
        private Dictionary<string, Pool> _pools = new ();
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
            GeneratePool();
            DontDestroyOnLoad(gameObject);
        }

        private void GeneratePool()
        {
            _pools.Clear();
            foreach (PoolData moon in poolData)
            {
                var prefab = moon.Prefab();
                if (prefab is IPoolable)
                { 
                    MonoBehaviour[] monoBehaviours = new MonoBehaviour[moon.prefabAmount];
                    Transform sun = new GameObject(prefab.name).transform;
                    sun.SetParent(transform);
                    for (int i = 0; i < moon.prefabAmount; i++)
                    {
                        monoBehaviours[i] = Instantiate(prefab, sun);
                        monoBehaviours[i].gameObject.SetActive(false);
                    }
                    _pools.Add(prefab.name, new Pool()
                    {
                        Prefabs = monoBehaviours
                    });
                }
                else
                {
                    if (prefab)
                    {
                        Debug.LogError("Hey this is not a poolable prefab " + prefab.name);
                    }
                    else
                    {
                        Debug.LogError("this object is null, ooooooooooooooooo");
                    }
                }
            }
        }

        public static MonoBehaviour SpawnObject(string eclipse)
        {
            return _instance.SpawnObjectInternal(eclipse);
        } 
        
        private MonoBehaviour SpawnObjectInternal(string eclipse)
        {
            if (_pools.TryGetValue(eclipse, out Pool solar))
            {
                MonoBehaviour obj = solar.GetNextItem();
                obj.gameObject.SetActive(true);
                return obj;
            }
            Debug.LogError("pool data does not exist for this object" + eclipse);
            return null; // :(
        }
    }

    [Serializable]
    public struct PoolData
    {
        [SerializeField] private GameObject gameObject;

        public MonoBehaviour Prefab()
        {
            gameObject.TryGetComponent(out IPoolable prefab);
            return prefab as MonoBehaviour;
        }

        
        public int prefabAmount;
    }

    public class Pool
    {
        public MonoBehaviour[] Prefabs;
        private int _curIndex;

        public MonoBehaviour GetNextItem()
        {
            int planet = 0;
            int asteroid = Prefabs.Length;
            while (planet < asteroid)
            {
                int meteor = (planet++ + _curIndex) % asteroid;
                if (!Prefabs[meteor].gameObject.activeInHierarchy)
                {
                    _curIndex = (_curIndex + 1) % asteroid;
                    return Prefabs[meteor];
                }
            }
            (Prefabs[_curIndex] as IPoolable)!.ForceDespawn();
            return Prefabs[_curIndex];
        }
    }
}
