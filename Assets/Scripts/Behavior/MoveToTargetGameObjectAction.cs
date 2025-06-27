using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move to Target GameObject", story: "[Agent] moves to [TargetGameObject]", category: "Action/Navigation", id: "f996f3a95b6a23011ebecb5f4ee4cbcb")]
    public partial class MoveToTargetGameObjectAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;

        private NavMeshAgent agent;
        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out agent) || TargetGameObject.Value == null)
            {
                return Status.Failure;
            }
            Vector3 targetPosition = GetTargetPosition();

            if (Vector3.Distance(agent.transform.position, targetPosition) <= agent.stoppingDistance)
            {
                return Status.Success;
            }

            agent.SetDestination(targetPosition);

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

        private Vector3 GetTargetPosition()
        {
            return TargetGameObject.Value.TryGetComponent(out Collider collider) ? collider.ClosestPoint(agent.transform.position) : TargetGameObject.Value.transform.position;
        }
    }
}