
using RTS.UI;
using RTS.Units;
using TMPro;
using UnityEngine;

class SingleUnitSelectedUI : MonoBehaviour, IUIElement<AbstractCommandable>
{
    [SerializeField] private TextMeshProUGUI nameText;
    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void EnableFor(AbstractCommandable commandable)
    {
        nameText.SetText(commandable.UnitSO.DisplayName); ;
        gameObject.SetActive(true);
    }
}