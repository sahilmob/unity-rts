using RTS.Units;
using UnityEngine;


namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Stop Action", menuName = "Units/Commands/Stop", order = 101)]
    public class StopCommand : BaseCommand
    {
        public override bool CanHandle(CommandContext ctx)
        {
            return ctx.Commandable is AbstractUnit;
        }

        public override void Handle(CommandContext ctx)
        {
            AbstractUnit unit = (AbstractUnit)ctx.Commandable;
            unit.Stop();
        }

    }
}