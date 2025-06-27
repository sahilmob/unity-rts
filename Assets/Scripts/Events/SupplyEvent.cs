using RTS.Environment;
using RTS.EventBus;

namespace RTS.Events
{
    public class SupplyEvent : IEvent
    {
        public int Amount { get; private set; }
        public SupplySO SupplySO { get; private set; }

        public SupplyEvent(int amount, SupplySO supplySO)
        {
            Amount = amount;
            SupplySO = supplySO;
        }
    }
}