using System;
using penguin;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        private PlayerController _player;

        private void Awake()
        {
            BindToPenguin(GameObject.FindObjectsByType<PlayerController>(FindObjectsSortMode.None)[0]);
        }

        public void BindToPenguin(PlayerController player)
        {
               _player = player;
               player.onHealthUpdated += ChangeHealthBar;
        }
        private void ChangeHealthBar()
        {
            healthBar.fillAmount = _player.Health / _player.PenguinoStats.Hp;
        }
    }
}
