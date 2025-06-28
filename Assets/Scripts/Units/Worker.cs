using RTS.Behavior;
using RTS.Environment;
using RTS.EventBus;
using RTS.Events;
using Unity.Behavior;
using UnityEngine;

namespace RTS.Units
{
    public class Worker : AbstractUnit, IBuildingBuilder
    {
        public bool HasSupplies
        {
            get
            {
                if (graphAgent != null && graphAgent.GetVariable("SupplyAmountHeld", out BlackboardVariable<int> SupplyAmountHeld))
                {
                    return SupplyAmountHeld > 0;
                }
                return false;
            }
        }
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

        public void ReturnSupplies(GameObject commandPost)
        {
            graphAgent.SetVariableValue("CommandPost", commandPost);
            graphAgent.SetVariableValue("Command", UnitCommand.ReturnSupplies);
        }


        public GameObject Build(BuildingSO buildingSO, Vector3 targetLocation)
        {
            GameObject instance = Instantiate(buildingSO.Prefab, targetLocation, Quaternion.identity);
            if (instance.TryGetComponent(out BaseBuilding baseBuilding))
            {
                baseBuilding.ShowGhostVisuals();
            }
            else
            {
                Debug.LogError($"Missing base building on Prefab for BuildingSO: {buildingSO.name}! Cannot build!");
                return null;
            }

            graphAgent.SetVariableValue("BuildingSO", buildingSO);
            graphAgent.SetVariableValue("TargetLocation", targetLocation);
            graphAgent.SetVariableValue("Ghost", instance);
            graphAgent.SetVariableValue("Command", UnitCommand.BuildBuilding);

            return instance;
        }

        private void HandleGatherSupplies(GameObject self, int amount, SupplySO supply)
        {
            Bus<SupplyEvent>.Raise(new(amount, supply));
        }
    }
}