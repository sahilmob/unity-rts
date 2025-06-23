using RTS.Commands;
using RTS.EventBus;
using RTS.Units;

namespace RTS.Events
{
    public struct ActionSelectedEvent : IEvent
    {
        public ActionBase Action { get; private set; }

        public ActionSelectedEvent(ActionBase action)
        {
            Action = action;
        }
    }
}
