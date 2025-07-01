using System;
using RTS.Commands;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RTS.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIActionButton : MonoBehaviour, IUIElement<BaseCommand, UnityAction>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private UITooltip tooltip;
        [SerializeField] private RectTransform RectTransform;
        private Button button;
        private bool isActive;

        void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            button = GetComponent<Button>();
            Disable();
        }

        public void EnableFor(BaseCommand command, UnityAction onClick)
        {
            isActive = true;
            button.onClick.RemoveAllListeners();
            tooltip?.SetText(GetTooltipText(command));
            SetIcon(command.Icon);
            button.interactable = !command.isLocked(new CommandContext());
            button.onClick.AddListener(onClick);
        }

        public void Disable()
        {
            isActive = false;
            SetIcon(null);
            button.interactable = false;
            button.onClick.RemoveAllListeners();
            tooltip?.Hide();
            CancelInvoke();
        }

        private void SetIcon(Sprite icon)
        {
            if (icon == null)
            {
                this.icon.enabled = false;
            }
            else
            {
                this.icon.sprite = icon;
                this.icon.enabled = true;
            }
        }

        public void OnPointerEnter(PointerEventData _)
        {
            if (isActive)
            {
                Invoke(nameof(ShowTooltip), 0.5f);
            }
        }

        public void OnPointerExit(PointerEventData _)
        {
            tooltip?.Hide();
            CancelInvoke(nameof(ShowTooltip));
        }

        private void ShowTooltip()
        {
            if (tooltip != null)
            {
                tooltip.RectTransform.position = new Vector2(RectTransform.position.x + RectTransform.rect.width / 2f, RectTransform.position.y + RectTransform.rect.height / 2);
                tooltip.Show();
            }
        }

        private string GetTooltipText(BaseCommand command)
        {
            string tooltipText = command.DisplayName + "\n";
            SupplyCostSO supplyCost = null;
            if (command is BuildUnitCommand unitCommand)
            {
                supplyCost = unitCommand.Unit.Cost;
            }
            else if (command is BuildBuildingCommand buildBuildingCommand)
            {
                supplyCost = buildBuildingCommand.BuildingSO.Cost;
            }

            if (supplyCost != null)
            {
                if (supplyCost.Minerals > 0)
                {
                    tooltipText += $"{supplyCost.Minerals} Minerals\n";
                }

                if (supplyCost.Gas > 0)
                {
                    tooltipText += $"{supplyCost.Gas} Gas\n";
                }
            }

            return tooltipText;
        }
    }
}
