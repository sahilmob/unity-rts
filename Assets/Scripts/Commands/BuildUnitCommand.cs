using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Build Unit", menuName = "Buildings/Commands/Build Unit", order = 120)]
    class BuildUnitCommand : ActionBase
    {
        [field: SerializeField] public AbstractUnitSO Unit { get; private set; }

        public override bool CanHandle(CommandContext ctx)
        {
            return ctx.Commandable is BaseBuilding;
        }

        public override void Handle(CommandContext ctx)
        {
            BaseBuilding building = (BaseBuilding)ctx.Commandable;
            building.BuildUnit(Unit); ;
        }
    }
}