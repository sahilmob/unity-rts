using RTS.Units;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Building is in progress", story: "[BaseBuilding] is being built", category: "Conditions", id: "fdb9e1cc514afbc24fad409259647c30")]
public partial class BuildingIsInProgressCondition : Condition
{
    [SerializeReference] public BlackboardVariable<BaseBuilding> BaseBuilding;

    public override bool IsTrue()
    {
        return BaseBuilding.Value != null && BaseBuilding.Value.Progress.State == BuildingProgress.BuildingState.Building;
    }
}
