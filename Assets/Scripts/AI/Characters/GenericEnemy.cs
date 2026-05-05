using Game.Objects;
using UnityEngine;

namespace AI.Characters
{
    public class GenericEnemy : MonoBehaviour, IDamageable
    {
#region Health
        [field: SerializeField] public float Health { get; set; }
        [field: SerializeField] public float DamageRes { get; private set; }
        public virtual void Die(Vector3 force)
        {
                    
        }
        public virtual void OnHurt(float amount, Vector3 force)
        {
                    
        }
#endregion        
        
        
        
        
    }
}
