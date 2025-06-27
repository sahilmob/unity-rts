using RTS.Environment;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using RTS.Utilities;

namespace RTS.Behavior
{

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Gather Supplies", story: "[Unit] gathers [Amount] supplies from [GatherableSupplies]", category: "Action/Units", id: "7af54aa14bb8543d3e4de93fed6ab7a0")]
    public partial class GatherSuppliesAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Unit;
        [SerializeReference] public BlackboardVariable<int> Amount;
        [SerializeReference] public BlackboardVariable<GatherableSupply> GatherableSupplies;
        [SerializeReference] public BlackboardVariable<SupplySO> SupplySO;
        private Animator animator;
        private float enterTime;

        protected override Status OnStart()
        {
            if (GatherableSupplies.Value == null)
            {
                return Status.Failure;
            }

            Unit.Value.TryGetComponent(out animator);

            animator?.SetBool(AnimationConstants.IS_GATHERING, true);

            enterTime = Time.time;
            GatherableSupplies.Value.BeginGather();
            SupplySO.Value = GatherableSupplies.Value.Supply;

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (GatherableSupplies.Value.Supply.BaseGatherTime + enterTime <= Time.time)
            {
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            animator?.SetBool(AnimationConstants.IS_GATHERING, false);
            if (GatherableSupplies.Value == null)
            {
                return;
            }

            if (CurrentStatus == Status.Success)
            {
                Amount.Value = GatherableSupplies.Value.EndGather();
            }
            else
            {
                GatherableSupplies.Value.Abort();
            }
        }
    }
}