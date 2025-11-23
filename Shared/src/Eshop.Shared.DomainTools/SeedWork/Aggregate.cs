using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using System.Text.Json.Serialization;

namespace EShop.Shared.DomainTools.SeedWork
{
    public interface IAggregate<TKey> : IEntityBase<TKey>
    {
        IEnumerable<IDomainEvent> UncommittedEvents { get; }
        void LoadFromHistory(IEnumerable<IDomainEvent> events);
    }

    public abstract class Aggregate<TKey> : IAggregate<TKey>
    {
        private readonly List<IDomainEvent> _uncommittedEvents = [];
        
        public TKey Id { get; set; } = default!;
        public ulong Version { get; private set; }

        [JsonIgnore]
        public IEnumerable<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

        public void LoadFromHistory(IEnumerable<IDomainEvent> events)
        {
            foreach (var @event in events)
            {
                Apply(@event);
                Version = @event.Version;
            }
        }

        protected void RaiseEvent(IDomainEvent @event)
        {
            @event.Version++;
            Apply(@event);
            _uncommittedEvents.Add(@event);
        }

        protected void Apply(IDomainEvent @event)
        {
            var method = this.GetType().GetMethod("Apply", [@event.GetType()]);
            if (method == null)
            {
                throw new InvalidOperationException($"Method Apply for {@event.GetType()} not found");
            }
            else
            {
                method.Invoke(this, [@event]);
            }
        }
    }
}