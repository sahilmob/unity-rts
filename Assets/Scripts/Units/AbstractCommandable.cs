using RTS.EventBus;
using RTS.Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace RTS.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable
    {
        [SerializeField] private int health;
        [field: SerializeField] public DecalProjector decalProjector { get; private set; }
        public void Deselect()
        {
            if (decalProjector == null) return;
            decalProjector.gameObject.SetActive(false);
            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }

        public void Select()
        {
            if (decalProjector == null) return;
            decalProjector.gameObject.SetActive(true);
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }
    }
}