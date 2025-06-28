using RTS.Commands;
using RTS.EventBus;
using RTS.Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace RTS.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable
    {
        [field: SerializeField] public int CurrentHealth { get; private set; }
        [field: SerializeField] public int MaxHealth { get; private set; }
        [field: SerializeField] public ActionBase[] AvailableCommands { get; private set; }
        [field: SerializeField] private DecalProjector decalProjector;
        [field: SerializeField] public UnitSO UnitSO { get; private set; }
        private ActionBase[] initialCommands;

        protected virtual void Start()
        {
            CurrentHealth = UnitSO.health;
            MaxHealth = UnitSO.health;
            initialCommands = AvailableCommands;
        }
        public void Deselect()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(false);
            }
            SetCommandOverrides(null);
            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }

        public void Select()
        {
            if (decalProjector == null) return;
            decalProjector.gameObject.SetActive(true);
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }

        public void SetCommandOverrides(ActionBase[] commands)
        {
            if (commands == null || commands.Length == 0)
            {
                AvailableCommands = initialCommands;
            }
            else
            {
                AvailableCommands = commands;
            }

            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }
    }
}