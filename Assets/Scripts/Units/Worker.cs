using RTS.Behavior;
using RTS.Environment;
using RTS.EventBus;
using RTS.Events;
using Unity.Behavior;
using UnityEngine;

namespace RTS.Units
{
    public class Worker : AbstractUnit
    {
        protected override void Start()
        {
            base.Start();

            if (graphAgent.GetVariable("GatherSuppliesEvent", out BlackboardVariable<GatherSuppliesEventChannel> evtChannel))
            {
                evtChannel.Value.Event += HandleGatherSupplies;
            }
        }

        public void Gather(GatherableSupply supply)
        {
            graphAgent.SetVariableValue("Supply", supply);
            graphAgent.SetVariableValue("TargetGameObject", supply.gameObject);
            graphAgent.SetVariableValue("Command", UnitCommand.Gather);
        }

        private void HandleGatherSupplies(GameObject self, int amount, SupplySO supply)
        {
            Bus<SupplyEvent>.Raise(new(amount, supply));
        }
    }
}