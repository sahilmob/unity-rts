using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
    public class BuildBuildingCommand : ActionBase
    {
        [field: SerializeField] public BuildingSO BuildingSO { get; private set; }
        public override bool CanHandle(CommandContext ctx)
        {
            if (ctx.Commandable is not IBuildingBuilder) return false;
            if (ctx.Hit.collider != null)
            {
                return ctx.Hit.collider.TryGetComponent(out BaseBuilding building)
                    && BuildingSO == building.BuildingSO
                    && (building.Progress.State == BuildingProgress.BuildingState.Paused
                        || building.Progress.State == BuildingProgress.BuildingState.Destroyed);
            }
            return true;
        }

        public override void Handle(CommandContext ctx)
        {
            IBuildingBuilder builder = (IBuildingBuilder)ctx.Commandable;
            if (ctx.Hit.collider != null && ctx.Hit.collider.TryGetComponent(out BaseBuilding building))
            {
                builder.ResumeBuilding(building);
            }
            else
            {
                builder.Build(BuildingSO, ctx.Hit.point);
            }
        }
    }
}