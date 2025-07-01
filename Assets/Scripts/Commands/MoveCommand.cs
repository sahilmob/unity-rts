using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{

    [CreateAssetMenu(fileName = "Move Action", menuName = "Units/Commands/Move", order = 100)]
    public class MoveCommand : BaseCommand
    {
        [SerializeField] private float radiusMultiplier = 3.5f;
        private int unitsOnLayer = 0;
        private int maxUnitsOnLayer = 1;
        private float circleRadius = 0;
        private float radialOffset = 0;
        public override bool CanHandle(CommandContext ctx)
        {
            return ctx.Commandable is AbstractUnit;
        }

        public override void Handle(CommandContext ctx)
        {
            RaycastHit hit = ctx.Hit;
            AbstractUnit unit = (AbstractUnit)ctx.Commandable;

            if (ctx.UnitIndex == 0)
            {
                unitsOnLayer = 0;
                maxUnitsOnLayer = 1;
                circleRadius = 0;
                radialOffset = 0;
            }


            Vector3 targetPosition = new(
                hit.point.x + circleRadius * Mathf.Cos(radialOffset * unitsOnLayer),
                hit.point.y,
                hit.point.z + circleRadius * Mathf.Sign(radialOffset * unitsOnLayer)
            );

            unit.MoveTo(targetPosition);
            unitsOnLayer++;

            if (unitsOnLayer >= maxUnitsOnLayer)
            {
                unitsOnLayer = 0;
                circleRadius += unit.AgentRadius * radiusMultiplier;
                maxUnitsOnLayer = Mathf.FloorToInt(2 * Mathf.PI * circleRadius / (unit.AgentRadius * 2));
                radialOffset = 2 * Mathf.PI / maxUnitsOnLayer;
            }
        }

        public override bool isLocked(CommandContext ctx)
        {
            return false;
        }
    }

}