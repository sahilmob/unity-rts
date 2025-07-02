

using System;
using RTS.UI;
using RTS.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UnitIconUI : MonoBehaviour, IUIElement<AbstractCommandable>
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI healthText;
    private AbstractCommandable commandable;
    private const string HEALTH_TEXT_FORMAT = "{0} / {1}";
    public void Disable()
    {
        gameObject.SetActive(false);
        if (commandable != null)
        {
            commandable.OnHealthUpdated -= HandleHealthUpdated;
            commandable = null;
        }
    }

    public void EnableFor(AbstractCommandable commandable)
    {
        gameObject.SetActive(true);
        icon.sprite = commandable.UnitSO.Icon;
        healthText.SetText(string.Format(HEALTH_TEXT_FORMAT, commandable.CurrentHealth, commandable.MaxHealth));
        this.commandable = commandable;
        commandable.OnHealthUpdated -= HandleHealthUpdated;
        commandable.OnHealthUpdated += HandleHealthUpdated;
    }

    private void HandleHealthUpdated(AbstractCommandable commandable, int lastHealth, int newHealth)
    {
        healthText.SetText(string.Format(HEALTH_TEXT_FORMAT, newHealth, commandable.MaxHealth));
    }
}