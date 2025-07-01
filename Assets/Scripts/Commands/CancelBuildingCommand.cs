

using RTS.Commands;
using RTS.Units;
using UnityEngine;

[CreateAssetMenu(fileName = "Cancel Building", menuName = "Units/Commands/Cancel Building")]
public class CancelBuildingCommand : BaseCommand
{
    public override bool CanHandle(CommandContext ctx)
    {
        return ctx.Commandable is IBuildingBuilder;
    }

    public override void Handle(CommandContext ctx)
    {
        IBuildingBuilder buildingBuilder = (IBuildingBuilder)ctx.Commandable;
        buildingBuilder.CancelBuilding();
    }

    public override bool isLocked(CommandContext ctx)
    {
        return false;
    }
}