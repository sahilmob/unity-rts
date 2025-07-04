using RTS.Player;
using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Build Unit", menuName = "Buildings/Commands/Build Unit", order = 120)]
    class BuildUnitCommand : BaseCommand
    {
        [field: SerializeField] public AbstractUnitSO Unit { get; private set; }

        public override bool CanHandle(CommandContext ctx)
        {
            return ctx.Commandable is BaseBuilding
                && HasEnoughSupplies();
        }

        public override void Handle(CommandContext ctx)
        {
            if (!HasEnoughSupplies()) return;
            BaseBuilding building = (BaseBuilding)ctx.Commandable;
            building.BuildUnit(Unit); ;
        }

        private bool HasEnoughSupplies()
        {
            return Unit?.Cost?.Minerals <= Supplies.Minerals
                && Unit?.Cost?.Gas <= Supplies.Gas;
        }

        public override bool isLocked(CommandContext ctx)
        {
            return !HasEnoughSupplies();
        }
    }
}