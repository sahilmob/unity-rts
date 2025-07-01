
using System;
using UnityEngine;
using System.Linq;


namespace RTS.Commands
{
    public abstract class BaseCommand : ScriptableObject, ICommand
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: Range(0, 8)] public int Slot;
        [field: SerializeField] public bool RequiresClickToActivate { get; private set; } = true;
        [field: SerializeField] public GameObject GhostPrefab { get; private set; }
        [field: SerializeField] public BuildingRestrictions[] Restrictions { get; private set; }
        public abstract bool CanHandle(CommandContext ctx);
        public abstract void Handle(CommandContext ctx);
        public abstract bool isLocked(CommandContext ctx);


        public bool AllRestrictionsPassed(Vector3 point)
        {
            return Restrictions.Aggregate(true, (acc, b) => acc && b.CanPlace(point));
        }
    }
}