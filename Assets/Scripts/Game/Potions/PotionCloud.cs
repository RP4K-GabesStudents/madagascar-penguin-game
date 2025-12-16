using System;
using UnityEngine;

namespace Game.Potions
{
    public class PotionCloud : MonoBehaviour
    {
        [SerializeField] private Material tempPotionCloud;

        public void InheritMaterial(Material masterial)
        {
            Color color = masterial.GetColor("_GradientTop");
            Color c = masterial.GetColor("_GradientBottom");
        }

        private void Awake()
        {
            InheritMaterial(tempPotionCloud);    
        }
    }
}
