
namespace RTS.EventBus
{
    public static class Bus<T> where T : IEvent
    {
        public delegate void Event(T args);
        public static event Event onEvent;

        public static void Raise(T e)
        {
            onEvent?.Invoke(e);
        }
    }
}