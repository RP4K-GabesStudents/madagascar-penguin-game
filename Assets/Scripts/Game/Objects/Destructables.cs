using Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Objects
{
    [SelectionBase]
    public class Destructables : MonoBehaviour, IDamageable
    {
        [SerializeField] private float dmgRes;
        [field: SerializeField] public float Health { get; set; }

        [SerializeField] private UnityEvent<Vector3, ForceMode> damaged;
        [SerializeField] private UnityEvent dead;
        public void Die()
        {
            dead?.Invoke();
            //Destroy(gameObject);
            Health = 12412;
            transform.position = new Vector3(116261, 61616, 623727);
        }

        public void OnHurt(float amount, Vector3 force)
        {
            damaged?.Invoke(force, ForceMode.Impulse);
        }

        public float DamageRes { get => dmgRes; }
    }
}
