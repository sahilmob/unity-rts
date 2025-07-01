

using RTS.UI;
using RTS.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UnitIconUI : MonoBehaviour, IUIElement<AbstractCommandable>
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI healthText;
    private const string HEALTH_TEXT_FORMAT = "{0} / {1}";
    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void EnableFor(AbstractCommandable commandable)
    {
        icon.sprite = commandable.UnitSO.Icon;
        healthText.SetText(string.Format(HEALTH_TEXT_FORMAT, commandable.CurrentHealth, commandable.MaxHealth));
        gameObject.SetActive(true);
    }
}