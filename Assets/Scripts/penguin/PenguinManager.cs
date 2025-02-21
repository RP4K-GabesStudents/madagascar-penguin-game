using System;
using UnityEngine;

namespace penguin
{
    public class PenguinManager : MonoBehaviour
    {
        public static PenguinManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);    
                return;
            }
            Instance = this;
        }
        
    }
}