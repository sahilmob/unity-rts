using System;
using System.Collections.Generic;
using System.Linq;
using RTS.Commands;
using RTS.EventBus;
using RTS.Events;
using RTS.UI.Components;
using RTS.Units;
using UnityEngine;
using UnityEngine.Events;

namespace RTS.UI.Container
{
    public class ActionsUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
    {
        [SerializeField] private UIActionButton[] actionButtons;

        private void RefreshButtons(HashSet<AbstractCommandable> selectedUnits)
        {
            HashSet<BaseCommand> actions = new(9);

            foreach (AbstractCommandable commandable in selectedUnits)
            {
                if (commandable.AvailableCommands != null)
                {
                    actions.UnionWith(commandable.AvailableCommands);
                }
            }

            for (int i = 0; i < actionButtons.Length; i++)
            {
                BaseCommand actionForSlot = actions.Where(a => a.Slot == i).FirstOrDefault();

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

        private UnityAction HandleClick(BaseCommand action)
        {
            return () => Bus<CommandSelectedEvent>.Raise(new CommandSelectedEvent(action));
        }

        public void EnableFor(HashSet<AbstractCommandable> selectedUnits)
        {
            RefreshButtons(selectedUnits);
        }

        public void Disable()
        {
            foreach (UIActionButton button in actionButtons)
            {
                button.Disable();
            }
        }
    }
}