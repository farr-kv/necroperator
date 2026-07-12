using Necroperator.Extensions;

namespace Necroperator.Services.Implementations
{
    internal class EventBus : IEventBus
    {
        delegate void InternalEvent(DateTimeOffset timestamp, object message);
        private event InternalEvent eventDelegate = delegate { };

        public IDisposable RegisterForEvent<T>(Action<DateTimeOffset, T> callback)
        {
            var del = new InternalEvent((timestamp, msg) =>
            {
                if (msg is T t)
                    callback(timestamp, t);
            });
            eventDelegate += del;
            return Disposable.Create(() => eventDelegate -= del);
        }

        public void Publish<T>(T message)
        {
            if (message is null)
                return;

            eventDelegate(DateTimeOffset.Now, message);
        }
    }
}
