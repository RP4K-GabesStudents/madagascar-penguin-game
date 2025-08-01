using Objects;
using UnityEngine;

namespace Game.Characters.Capabilities
{
    [CreateAssetMenu(fileName = "InteractionCapabilityStats", menuName = "Characters/CapabilityStats/InteractionCapabilityStats")]
    public class InteractionCapabilityStats : CapabilityStats
    {
       // public override Type GetCapabilityType() => typeof(CrouchCapability);
       [SerializeField, Range(0, 1)] private float interactionRadius = 0;
       [SerializeField, Range(0, 10)] private float interactionDistance;

       [SerializeField] private LayerMask interactionLayers;
       [SerializeField] private LayerMask blockingLayers;

       public float  InteractionRadius => interactionRadius;
       public float InteractionDistance => interactionDistance;
       public LayerMask InteractionLayers => interactionLayers;
       public LayerMask BlockingLayers => blockingLayers;
       public LayerMask CombinedLayers =>  InteractionLayers | BlockingLayers;
    }
}