namespace RTS.UI
{
    public interface IUIElement<T>
    {
        void EnableFor(T item);
        void Disable();
    }

    public interface IUIElement<T, U>
    {
        void EnableFor(T item, U callback);
        void Disable();
    }
}