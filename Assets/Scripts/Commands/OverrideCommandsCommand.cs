
using RTS.Units;
using UnityEngine;
namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Override Commands", menuName = "Units/Commands/Override Commands", order = 110)]
    public class OverrideCommandsCommand : ActionBase
    {
        [field: SerializeField] public ActionBase[] Commands { get; private set; }
        public override bool CanHandle(CommandContext ctx)
        {
            return ctx.Commandable != null;
        }

        public override void Handle(CommandContext ctx)
        {
            ctx.Commandable.SetCommandOverrides(Commands);
        }
    }
}