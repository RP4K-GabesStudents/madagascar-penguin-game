using System;
using penguin;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        [SerializeField] private PlayerController player;

        private void Start()
        {
            BindToPenguin(player);
        }

        public void BindToPenguin(PlayerController player)
        {
               this.player = player;
               player.onHealthUpdated += ChangeHealthBar;
        }
        private void ChangeHealthBar()
        {
            healthBar.fillAmount = player.Health / player.PenguinoStats.Hp;
        }
    }
}
