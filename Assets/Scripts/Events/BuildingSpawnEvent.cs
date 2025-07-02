

using RTS.EventBus;
using RTS.Units;

public class BuildingSpawnEvent : IEvent
{
    public BaseBuilding Building { get; private set; }

    public BuildingSpawnEvent(BaseBuilding building)
    {
        Building = building;
    }
}