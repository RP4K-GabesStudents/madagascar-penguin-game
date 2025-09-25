using System;
using System.Collections.Generic;
using Detection;

namespace Game.Characters.World
{
    public class Penguin : GenericCharacter, IDetectable
    {
        public static readonly List<Penguin> Penguins = new ();
        public override void OnNetworkSpawn()
        {]
        }

        private void OnEnable()
        {
            Penguins.Add(this);
        }

        private void OnDisable()
        {
            Penguins.Remove(this);
        }

        public void OnDetected()
        {
            
        }
    }
}