using System;
using RTS.Behavior;
using RTS.Commands;
using RTS.Environment;
using RTS.EventBus;
using RTS.Events;
using Unity.Behavior;
using Unity.VisualScripting;
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

        public bool IsBuilding => graphAgent.GetVariable("Command", out BlackboardVariable<UnitCommand> command) && command.Value == UnitCommand.BuildBuilding;

        [SerializeField] private BaseCommand CancelBuildingCommand;

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
            if (!instance.TryGetComponent(out BaseBuilding _))
            {
                Debug.LogError($"Missing base building on Prefab for BuildingSO: {buildingSO.name}! Cannot build!");
                return null;
            }

            graphAgent.SetVariableValue("BuildingSO", buildingSO);
            graphAgent.SetVariableValue("TargetLocation", targetLocation);
            graphAgent.SetVariableValue("Ghost", instance);
            graphAgent.SetVariableValue("Command", UnitCommand.BuildBuilding);

            SetCommandOverrides(new BaseCommand[] { CancelBuildingCommand });
            // Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
            Bus<SupplyEvent>.Raise(new(-buildingSO.Cost.Gas, buildingSO.Cost.GasSO));
            Bus<SupplyEvent>.Raise(new(-buildingSO.Cost.Minerals, buildingSO.Cost.MineralsSO));
            return instance;
        }

        public void ResumeBuilding(BaseBuilding building)
        {
            graphAgent.SetVariableValue("TargetLocation", building.transform.position);
            graphAgent.SetVariableValue("BuildingUnderConstruction", building);
            graphAgent.SetVariableValue("BuildingSO", building.BuildingSO);
            graphAgent.SetVariableValue<GameObject>("Ghost", null);
            graphAgent.SetVariableValue("Command", UnitCommand.BuildBuilding);

            SetCommandOverrides(new BaseCommand[] { CancelBuildingCommand });
            Bus<UnitSelectedEvent>.Raise(new(this));
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
                BuildingSO buildingSO = buildingUnderConstruction.Value.BuildingSO;
                Bus<SupplyEvent>.Raise(new(Mathf.FloorToInt(buildingSO.Cost.Gas * 0.75f), buildingSO.Cost.GasSO));
                Bus<SupplyEvent>.Raise(new(Mathf.FloorToInt(buildingSO.Cost.Minerals * 0.75f), buildingSO.Cost.MineralsSO));
            }

            SetCommandOverrides(Array.Empty<BaseCommand>());

            Stop();
        }
    }
}