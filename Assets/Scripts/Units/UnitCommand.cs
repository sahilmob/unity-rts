using Unity.Behavior;

namespace RTS.Units
{
    [BlackboardEnum]
    public enum UnitCommand
    {
        Stop,
        Move,
        Gather,
        ReturnSupplies
    }
}