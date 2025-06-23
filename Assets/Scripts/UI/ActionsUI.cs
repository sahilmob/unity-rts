using System;
using System.Collections.Generic;
using System.Linq;
using RTS.Commands;
using RTS.EventBus;
using RTS.Events;
using RTS.Units;
using UnityEngine;
using UnityEngine.Events;

namespace RTS.UI
{
    public class ActionsUI : MonoBehaviour
    {
        [SerializeField] private UIActionButton[] actionButtons;
        private HashSet<AbstractCommandable> selectedUnit = new(12);

        private void Awake()
        {
            Bus<UnitSelectedEvent>.onEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.onEvent += HandleUnitDeSelected;
        }

        private void Start()
        {
            foreach (UIActionButton button in actionButtons)
            {
                button.Disable();
            }
        }


        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.onEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.onEvent -= HandleUnitDeSelected;
        }

        private void HandleUnitDeSelected(UnitDeselectedEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
            {
                selectedUnit.Remove(commandable);
                RefreshButtons();
            }
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
            {
                selectedUnit.Add(commandable);
                RefreshButtons();
            }
        }

        private void RefreshButtons()
        {
            HashSet<ActionBase> actions = new(9);

            foreach (AbstractCommandable commandable in selectedUnit)
            {
                actions.UnionWith(commandable.AvailableCommands);
            }

            for (int i = 0; i < actionButtons.Length; i++)
            {
                ActionBase actionForSlot = actions.Where(a => a.Slot == i).FirstOrDefault();

                if (actionForSlot != null)
                {
                    actionButtons[i].EnableFor(actionForSlot, HandleClick(actionForSlot));
                }
                else
                {
                    actionButtons[i].Disable();
                }
            }
        }

        private UnityAction HandleClick(ActionBase action)
        {
            return () => Bus<ActionSelectedEvent>.Raise(new ActionSelectedEvent(action));
        }
    }
}