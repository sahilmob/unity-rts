

using RTS.EventBus;
using RTS.UI;
using RTS.UI.Container;
using RTS.Units;
using UnityEngine;

public class BuildingSelectedUI : MonoBehaviour, IUIElement<BaseBuilding>
{
    [SerializeField] private SingleUnitSelectedUI singleUnitSelectedUI;
    [SerializeField] private BuildingBuildingUI buildingBuildingUI;
    [SerializeField] private BuildingUnderConstructionUI buildingUnderConstructionUI;

    private BaseBuilding selectedBuilding;

    private void HandleBuildingSpawn(BuildingSpawnEvent e)
    {
        if (selectedBuilding == e.Building)
        {
            EnableFor(e.Building);
            Bus<BuildingSpawnEvent>.onEvent -= HandleBuildingSpawn;
        }
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        singleUnitSelectedUI.Disable();
        buildingUnderConstructionUI.Disable();
        Bus<BuildingSpawnEvent>.onEvent -= HandleBuildingSpawn;
        buildingBuildingUI.Disable();
        if (selectedBuilding != null)
        {
            selectedBuilding.OnQueueUpdated -= OnBuildingQueueUpdated;
            selectedBuilding = null;
        }
    }

    public void EnableFor(BaseBuilding building)
    {
        gameObject.SetActive(true);
        selectedBuilding = building;
        selectedBuilding.OnQueueUpdated -= OnBuildingQueueUpdated;
        selectedBuilding.OnQueueUpdated += OnBuildingQueueUpdated;
        if (building.Progress.State == BuildingProgress.BuildingState.Completed)
        {
            buildingUnderConstructionUI.Disable();
            OnBuildingQueueUpdated();
        }
        else
        {
            singleUnitSelectedUI.Disable();
            buildingBuildingUI.Disable();
            buildingUnderConstructionUI.EnableFor(building);
            Bus<BuildingSpawnEvent>.onEvent += HandleBuildingSpawn;
        }
    }

    private void OnBuildingQueueUpdated(AbstractUnitSO[] _ = null)
    {
        if (selectedBuilding.QueueSize > 0)
        {
            singleUnitSelectedUI.Disable();
            buildingBuildingUI.EnableFor(selectedBuilding);
        }
        else
        {
            buildingBuildingUI.Disable();
            singleUnitSelectedUI.EnableFor(selectedBuilding);
        }
    }
}