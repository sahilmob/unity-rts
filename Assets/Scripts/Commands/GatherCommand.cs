using RTS.Environment;
using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Gather Action", menuName = "Units/Commands/Gather", order = 105)]
    class GatherCommand : ActionBase
    {
        [SerializeField] private UnitSO commandPostSO;
        public override bool CanHandle(CommandContext ctx)
        {
            return ctx.Commandable is Worker
                && ctx.Hit.collider != null
                && IsGatherableSupplyOrCommandPost(ctx.Hit.collider);
        }

        public override void Handle(CommandContext ctx)
        {
            Worker worker = (Worker)ctx.Commandable;
            if (ctx.Hit.collider.TryGetComponent(out GatherableSupply supply))
            {
                worker.Gather(supply);
            }
            else if (IsCommandPost(ctx.Hit.collider) && worker.HasSupplies)
            {
                worker.ReturnSupplies(ctx.Hit.collider.gameObject);
            }
            else
            {
                worker.MoveTo(ctx.Hit.collider.gameObject.transform.position);
            }
        }

        private bool IsGatherableSupplyOrCommandPost(Collider collider)
        {
            return collider.TryGetComponent(out GatherableSupply _) || IsCommandPost(collider);
        }
        private bool IsCommandPost(Collider collider)
        {
            return collider.TryGetComponent(out BaseBuilding b)
                    && b.UnitSO.Equals(commandPostSO);
        }
    }
}