using Game.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        [SerializeField] private GenericCharacter owner;


        public void BindToOwner(GenericCharacter newOwner)
        {
               owner = newOwner;
               newOwner.OnHealthUpdated += ChangeHealthBar;
        }
        private void ChangeHealthBar(float _)
        {
            healthBar.fillAmount = owner.HealthPercent;
        }
    }
}
