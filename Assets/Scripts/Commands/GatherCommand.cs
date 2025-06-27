using RTS.Environment;
using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Gather Action", menuName = "AI/Commands/Gather", order = 105)]
    class GatherCommand : ActionBase
    {
        public override bool CanHandle(CommandContext ctx)
        {
            return ctx.Commandable is Worker && ctx.Hit.collider != null && ctx.Hit.collider.TryGetComponent(out GatherableSupply _);
        }

        public override void Handle(CommandContext ctx)
        {
            Worker worker = (Worker)ctx.Commandable;
            worker.Gather(ctx.Hit.collider.GetComponent<GatherableSupply>());
        }
    }
}