using System;
using System.Collections.Generic;
using Detection;
using Detection.Core;
using UnityEngine;

namespace Game.Characters.World
{
    public class Penguin : GenericCharacter, IDetectable
    {
        public static readonly List<Penguin> Penguins = new ();
        public override void OnNetworkSpawn()
        {
            if(IsOwner) PlayerController.Instance.SubscribeTo(gameObject);
        }

        private void OnEnable()
        {
            Penguins.Add(this);
        }

        private void OnDisable()
        {
            Penguins.Remove(this);
        }

        public void OnDetectedBy(MonoBehaviour detector)
        {
            Debug.Log("OnDetected", gameObject);
        }

        public void OnDetectionLost(MonoBehaviour detector)
        {
            Debug.Log("OnDetectionLost", gameObject);
        }
    }
}