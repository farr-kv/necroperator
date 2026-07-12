namespace Necroperator.Services
{
    public interface IEventBus
    {
        IDisposable RegisterForEvent<T>(Action<DateTimeOffset, T> callback);
        void Publish<T>(T message);
    }
}