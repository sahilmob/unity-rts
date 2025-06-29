using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SamplePosition", story: "Set [TargetLocation] to closest point on the NavMesh to [Target]", category: "Action/Navigation", id: "3b7f6f8b2f00a40f253a0b060d54cb38")]
public partial class SamplePositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Radius = new(5);

    protected override Status OnStart()
    {
        if (Target.Value == null || !Target.Value.TryGetComponent(out NavMeshAgent agent)) return Status.Failure;

        NavMeshQueryFilter queryFilter = new();
        queryFilter.agentTypeID = agent.agentTypeID;
        queryFilter.areaMask = agent.areaMask;

        if (NavMesh.SamplePosition(Target.Value.transform.position, out NavMeshHit hit, Radius, queryFilter))
        {
            TargetLocation.Value = hit.position;
            return Status.Success;
        }

        return Status.Failure;
    }

}

