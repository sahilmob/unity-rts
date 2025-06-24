using System.Collections.Generic;
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
        [SerializeField] public ActionsUI actionsUi;
        private void Awake()
        {
            Bus<UnitSelectedEvent>.onEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.onEvent += HandleUnitDeselected;
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
                actionsUi.Disable();
            else
                actionsUi.EnableFor(selectedUnits);
        }

        private void HandleUnitSelected(UnitSelectedEvent e)
        {
            if (e.Unit is AbstractCommandable unit)
            {
                selectedUnits.Add(unit);
                actionsUi.EnableFor(selectedUnits);
            }
        }
    }
}