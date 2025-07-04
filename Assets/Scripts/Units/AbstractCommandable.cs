using System;
using RTS.Commands;
using RTS.EventBus;
using RTS.Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace RTS.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable, IDamageable
    {
        [field: SerializeField] public bool IsSelected { get; protected set; }
        [field: SerializeField] public int CurrentHealth { get; protected set; }
        [field: SerializeField] public int MaxHealth { get; protected set; }
        [field: SerializeField] public BaseCommand[] AvailableCommands { get; private set; }
        [field: SerializeField] protected DecalProjector decalProjector;
        [field: SerializeField] public AbstractUnitSO UnitSO { get; private set; }
        public Transform Transform => transform;
        public delegate void HealthUpdatedEvent(AbstractCommandable commandable, int lastHealth, int newHealth);
        public event HealthUpdatedEvent OnHealthUpdated;
        private BaseCommand[] initialCommands;

        protected virtual void Start()
        {
            initialCommands = AvailableCommands;
        }
        public virtual void Deselect()
        {
            decalProjector?.gameObject?.SetActive(false);
            SetCommandOverrides(null);
            IsSelected = false;
            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }

        public virtual void Select()
        {
            decalProjector?.gameObject?.SetActive(true);
            IsSelected = true;
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }

        public void SetCommandOverrides(BaseCommand[] commands)
        {
            if (commands == null || commands.Length == 0)
            {
                AvailableCommands = initialCommands;
            }
            else
            {
                AvailableCommands = commands;
            }

            if (IsSelected)
            {
                Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
            }
        }

        public void Heal(int amount)
        {
            int lastHealth = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
            OnHealthUpdated?.Invoke(this, lastHealth, CurrentHealth);
        }

        public void TakeDamage(int damage)
        {
            int lastHealth = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, CurrentHealth);
            OnHealthUpdated?.Invoke(this, lastHealth, CurrentHealth);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            Destroy(gameObject);
        }
    }
}