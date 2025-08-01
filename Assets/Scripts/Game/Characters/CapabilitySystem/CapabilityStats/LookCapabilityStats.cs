using UnityEngine;

namespace Game.Characters.CapabilitySystem.CapabilityStats
{
    [CreateAssetMenu(fileName = "LookCapabilityStats", menuName = "Characters/CapabilityStats/LookCapabilityStats")]
    public class LookCapabilityStats : Characters.CapabilityStats
    {
       // public override Type GetCapabilityType() => typeof(GroundMovementCapabilityStats);
       [Header("Looking")] 
       [SerializeField, Min(0)] private float rotationSpeed;
       [SerializeField, Range(0,90)] private float pitchLimit;
        
       [Header("Animation")]
       [SerializeField, Min(0)] private float rotationAnimationSpeed;
       [SerializeField, Min(0)] private float animationReturnSpeed;
       [SerializeField, Range(-1,1)] private float rotationAnimationThreshold;
       
       public float RotationSpeed => rotationSpeed;
       public float PitchLimit => pitchLimit;
       
       public float RotationAnimationSpeed => rotationAnimationSpeed;
       public float AnimationReturnSpeed => animationReturnSpeed;
       public float RotationAnimationThreshold => rotationAnimationThreshold;
    }
}