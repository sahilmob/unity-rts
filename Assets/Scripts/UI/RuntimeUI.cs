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
        [SerializeField] BuildingSelectedUI buildingSelectedUI;
        [SerializeField] private UnitIconUI unitIconUI;
        [SerializeField] private SingleUnitSelectedUI unitSelectedUI;
        private void Awake()
        {
            Bus<UnitSelectedEvent>.onEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.onEvent += HandleUnitDeselected;
            Bus<UnitDeathEvent>.onEvent += HandleUnitDeath;
            Bus<SupplyEvent>.onEvent += HandleSupplyEvent;
        }

        private void Start()
        {
            actionsUI.Disable();
            buildingSelectedUI.Disable();
            unitIconUI.Disable();
            unitSelectedUI.Disable();
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.onEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.onEvent -= HandleUnitDeselected;
            Bus<UnitDeathEvent>.onEvent -= HandleUnitDeath;
            Bus<SupplyEvent>.onEvent -= HandleSupplyEvent;
        }

        private void HandleUnitDeath(UnitDeathEvent e)
        {
            Bus<UnitDeselectedEvent>.Raise(new(e.Unit));
        }

        private void HandleUnitDeselected(UnitDeselectedEvent e)
        {
            if (e.Unit is AbstractCommandable unit)
            {
                selectedUnits.Remove(unit);
            }

            RefreshUI();
        }

        private void RefreshUI()
        {
            if (selectedUnits.Count == 0)
            {
                actionsUI.Disable();
                buildingSelectedUI.Disable();
                unitIconUI.Disable();
                unitSelectedUI.Disable();
            }
            else
            {
                actionsUI.EnableFor(selectedUnits);

                if (selectedUnits.Count == 1)
                {
                    AbstractCommandable commandable = selectedUnits.First();
                    unitIconUI.EnableFor(commandable);
                    unitSelectedUI.EnableFor(commandable);

                    if (commandable is BaseBuilding building)
                    {
                        unitSelectedUI.Disable();
                        buildingSelectedUI.EnableFor(building);
                    }
                    else
                    {
                        buildingSelectedUI.Disable();
                        unitSelectedUI.EnableFor(commandable);
                    }
                }
                else
                {
                    unitIconUI.Disable();
                    unitSelectedUI.Disable();
                    buildingSelectedUI.Disable();
                }
            }
        }

        private void HandleUnitSelected(UnitSelectedEvent e)
        {
            if (e.Unit is AbstractCommandable unit)
            {
                selectedUnits.Add(unit);
                RefreshUI();
            }
        }

        private void HandleSupplyEvent(SupplyEvent e)
        {
            actionsUI.EnableFor(selectedUnits);
        }
    }
}