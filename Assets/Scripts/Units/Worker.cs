using System;
using RTS.Behavior;
using RTS.Commands;
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
        [SerializeField] private ActionBase CancelBuildingCommand;

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
                baseBuilding.StartBuilding(this);
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

            SetCommandOverrides(new ActionBase[] { CancelBuildingCommand });
            // Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
            return instance;
        }

        private void HandleGatherSupplies(GameObject self, int amount, SupplySO supply)
        {
            Bus<SupplyEvent>.Raise(new(amount, supply));
        }

        public void CancelBuilding()
        {
            if (graphAgent.GetVariable("Ghost", out BlackboardVariable<GameObject> ghost) && ghost.Value != null)
            {
                Destroy(ghost.Value);
            }
            if (graphAgent.GetVariable("BuildingUnderConstruction", out BlackboardVariable<BaseBuilding> buildingUnderConstruction) && buildingUnderConstruction.Value != null)
            {
                Destroy(buildingUnderConstruction.Value.gameObject);
            }

            SetCommandOverrides(Array.Empty<ActionBase>());

            Stop();
        }
    }
}