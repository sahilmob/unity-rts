
using RTS.Units;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace RTS.UI.Components
{

    public class UIBuildQueueButton : MonoBehaviour, IUIElement<AbstractUnitSO, UnityAction>
    {
        private Button button;
        [SerializeField] private Image icon;

        private void Awake()
        {
            button = GetComponent<Button>();
            Disable();
        }

        public void Disable()
        {
            button.interactable = false;
            button.onClick.RemoveAllListeners();
            icon.gameObject.SetActive(false);
            icon.sprite = null;
        }

        public void EnableFor(AbstractUnitSO item, UnityAction callback)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = true;
            button.onClick.AddListener(callback);
            icon.gameObject.SetActive(true);
            icon.sprite = item.Icon;
        }
    }
}