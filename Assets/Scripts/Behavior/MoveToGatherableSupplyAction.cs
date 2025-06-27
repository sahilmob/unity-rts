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
        private SupplySO supplySO;
        private NavMeshAgent agent;
        private Animator animator;
        protected override Status OnStart()
        {
            suppliesMask = LayerMask.GetMask("Supplies");

            if (!HasValidInputs())
            {
                return Status.Failure;
            }

            Agent.Value.TryGetComponent(out animator);

            Vector3 targetPosition = GetTargetPosition();

            agent.SetDestination(targetPosition);

            return Status.Running;
        }

        private bool HasValidInputs()
        {
            if (!Agent.Value.TryGetComponent(out agent) || (Supply.Value == null && supplySO == null))
            {
                return false;
            }

            if (Supply.Value != null)
            {
                supplySO = Supply.Value.Supply;
            }
            else
            {
                Collider[] colliders = FindNearbyNotBusyColliders();
                if (colliders.Length > 1)
                {
                    Array.Sort(colliders, new ClosestColliderComparer(agent.transform.position));
                }
                if (colliders.Length > 0)
                {
                    Supply.Value = colliders[0].GetComponent<GatherableSupply>();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        protected override Status OnUpdate()
        {
            animator?.SetFloat(AnimationConstants.SPEED, agent.velocity.magnitude);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                return Status.Running;
            }


            if (Supply.Value != null && !Supply.Value.IsBusy && Supply.Value.Amount > 0)
            {
                return Status.Success;
            }

            Collider[] colliders = FindNearbyNotBusyColliders();

            if (colliders.Length > 1)
            {
                Array.Sort(colliders, new ClosestColliderComparer(agent.transform.position));
            }
            if (colliders.Length > 0)
            {
                Supply.Value = colliders[0].GetComponent<GatherableSupply>();
                agent.SetDestination(GetTargetPosition());
                return Status.Running;
            }

            return Status.Failure;
        }

        private Collider[] FindNearbyNotBusyColliders()
        {
            return Physics.OverlapSphere(agent.transform.position, SearchRadius, suppliesMask)
                .Where(c => c.TryGetComponent(out GatherableSupply s)
                    && !s.IsBusy
                    && s.Supply.Equals(supplySO)).ToArray();
        }

        private Vector3 GetTargetPosition()
        {
            return Supply.Value.TryGetComponent(out Collider collider) ? collider.ClosestPoint(agent.transform.position) : Supply.Value.transform.position;
        }

        protected override void OnEnd()
        {
            animator?.SetFloat(AnimationConstants.SPEED, 0);
        }
    }
}