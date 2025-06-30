using RTS.Commands;
using RTS.EventBus;
using RTS.Units;

namespace RTS.Events
{
    public struct CommandSelectedEvent : IEvent
    {
        public BaseCommand Command { get; private set; }

        public CommandSelectedEvent(BaseCommand command)
        {
            Command = command;
        }
    }
}
