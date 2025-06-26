using System.Collections.Generic;
using System.Linq;
using RTS.EventBus;
using RTS.Events;
using RTS.UI.Container;
using RTS.Units;
using UnityEngine;

namespace RTS.UI
{
    public class RuntimeUI : MonoBehaviour
    {
        private HashSet<AbstractCommandable> selectedUnits = new();
        [SerializeField] private ActionsUI actionsUI;
        [SerializeField] private BuildingBuildingUI buildingBuildingUI;
        private void Awake()
        {
            Bus<UnitSelectedEvent>.onEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.onEvent += HandleUnitDeselected;
        }

        private void Start()
        {
            actionsUI.Disable();
            buildingBuildingUI.Disable();
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.onEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.onEvent -= HandleUnitDeselected;
        }

        private void HandleUnitDeselected(UnitDeselectedEvent e)
        {
            if (e.Unit is AbstractCommandable unit)
            {
                selectedUnits.Remove(unit);
            }

            if (selectedUnits.Count == 0)
            {
                actionsUI.Disable();
                buildingBuildingUI.Disable();
            }
            else
            {
                actionsUI.EnableFor(selectedUnits);
                if (selectedUnits.Count == 1 && selectedUnits.First() is BaseBuilding building)
                {
                    buildingBuildingUI.EnableFor(building);
                }
                else
                {
                    buildingBuildingUI.Disable();
                }
            }
        }

        private void HandleUnitSelected(UnitSelectedEvent e)
        {
            if (e.Unit is AbstractCommandable unit)
            {
                selectedUnits.Add(unit);
                actionsUI.EnableFor(selectedUnits);
            }

            if (selectedUnits.Count == 1 && e.Unit is BaseBuilding building)
            {
                buildingBuildingUI.EnableFor(building);
            }
        }
    }
}