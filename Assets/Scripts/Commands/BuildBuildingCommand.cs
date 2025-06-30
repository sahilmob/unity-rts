using RTS.Units;
using UnityEngine;
using RTS.Player;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
    public class BuildBuildingCommand : BaseCommand
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

            return HasEnoughSupplies() && AllRestrictionsPassed(ctx.Hit.point);
        }

        public override void Handle(CommandContext ctx)
        {
            IBuildingBuilder builder = (IBuildingBuilder)ctx.Commandable;
            if (ctx.Hit.collider != null && ctx.Hit.collider.TryGetComponent(out BaseBuilding building))
            {
                builder.ResumeBuilding(building);
            }
            else if (HasEnoughSupplies() && AllRestrictionsPassed(ctx.Hit.point))
            {
                builder.Build(BuildingSO, ctx.Hit.point);
            }
        }

        private bool HasEnoughSupplies()
        {
            return BuildingSO.Cost.Minerals <= Supplies.Minerals
                && BuildingSO.Cost.Gas <= Supplies.Gas;
        }
    }
}