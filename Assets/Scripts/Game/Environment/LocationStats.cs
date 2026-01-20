using UnityEngine;

namespace Game.Environment
{
    [CreateAssetMenu(fileName = "LocationStats", menuName = "Scriptable Objects/LocationStats")]
    public class LocationStats : ScriptableObject
    {
        [SerializeField] private Transform location;
        [SerializeField] private float radius;
        [SerializeField] private float healAmount;
        [SerializeField] private float rechargeAmount;
        
        
        
        public Transform Location => location;
        public float Radius => radius;
        public float HealAmount => healAmount;
        public float RechargeAmount => rechargeAmount;
    }
}
