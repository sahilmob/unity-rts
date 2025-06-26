using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;


namespace RTS.Behavior
{

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move To Target Location", story: "[Agent] moves to [TargetLocation]", category: "Action/Navigation", id: "236016284fae1d87bfd49ef13d6fc94a")]
    public partial class MoveToTargetLocationAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;

        private NavMeshAgent agent;
        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out agent))
            {
                return Status.Failure;
            }

            if (Vector3.Distance(agent.transform.position, TargetLocation.Value) <= agent.stoppingDistance)
            {
                return Status.Success;
            }

            agent.SetDestination(TargetLocation.Value);

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                return Status.Success;
            }
            return Status.Running;
        }

    }

}

