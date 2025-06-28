using RTS.Units;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "PickRandomLocationWithinRendererBounds", story: "Set [TargetLocation] to a random point within [BuildingUnderConstruction]", category: "Action", id: "fda1048850b32eebba44ecc6c5bbcc0f")]
    public partial class PickRandomLocationWithinRendererBoundsAction : Action
    {
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        [SerializeReference] public BlackboardVariable<BaseBuilding> BuildingUnderConstruction;
        protected override Status OnStart()
        {
            if (BuildingUnderConstruction.Value == null)
            {
                return Status.Failure;
            }

            Renderer renderer = BuildingUnderConstruction.Value.MainRenderer;
            if (renderer == null)
            {
                return Status.Failure;
            }

            Bounds bounds = renderer.bounds;

            TargetLocation.Value = new Vector3(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                TargetLocation.Value.y,
                UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
            );

            return Status.Success;
        }

    }
}