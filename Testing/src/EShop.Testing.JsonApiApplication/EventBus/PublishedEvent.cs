using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Testing.JsonApiApplication.EventBus
{
    public sealed class PublishedEvent(object @event)
    {
        public object Event { get; } = @event;

        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    }
}