using RTS.Environment;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using System.Linq;
using RTS.Utilities;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move to GatherableSupply", story: "[Agent] moves to [Supply] or nearby not busy Supply", category: "Action/Navigation", id: "e1cc323f739f4f03d8411a34a7c8a8c8")]
    public partial class MoveToGatherableSupplyAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GatherableSupply> Supply;
        [SerializeReference] public BlackboardVariable<float> SearchRadius = new(7f);
        private LayerMask suppliesMask;

        private NavMeshAgent agent;
        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out agent))
            {
                return Status.Failure;
            }

            suppliesMask = LayerMask.GetMask("Supplies");
            Vector3 targetPosition = GetTargetPosition();

            agent.SetDestination(targetPosition);


            return Status.Running;
        }

        protected override Status OnUpdate()
        {

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                return Status.Running;
            }

            if (!Supply.Value.IsBusy && Supply.Value.Amount > 0)
            {
                return Status.Success;
            }

            Collider[] colliders = Physics.OverlapSphere(agent.transform.position, SearchRadius, suppliesMask)
                .Where(c => c.TryGetComponent(out GatherableSupply s)
                    && !s.IsBusy
                    && s.Supply.Equals(Supply.Value.Supply)).ToArray();


            if (colliders.Length > 0)
            {
                Array.Sort(colliders, new ClosestColliderComparer(agent.transform.position));
                Supply.Value = colliders[0].GetComponent<GatherableSupply>();
                agent.SetDestination(GetTargetPosition());
                return Status.Running;
            }

            return Status.Failure;
        }

        private Vector3 GetTargetPosition()
        {
            return Supply.Value.TryGetComponent(out Collider collider) ? collider.ClosestPoint(agent.transform.position) : Supply.Value.transform.position;
        }
    }
}