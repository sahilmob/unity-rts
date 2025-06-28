using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
    public class BuildBuildingCommand : ActionBase
    {
        [field: SerializeField] public BuildingSO buildingSO { get; private set; }
        public override bool CanHandle(CommandContext ctx)
        {
            return ctx.Commandable is IBuildingBuilder;
        }

        public override void Handle(CommandContext ctx)
        {
            IBuildingBuilder builder = (IBuildingBuilder)ctx.Commandable;
            builder.Build(buildingSO, ctx.Hit.point);
        }
    }
}