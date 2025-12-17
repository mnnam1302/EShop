using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Catalog.Application.Products.Update
{
    internal class ProductUpdatedEvent : IDomainEvent
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid CategoryId { get; set; }
        public string[] Tags { get; set; }
        public string Slug { get; set; }
        public string[] Images { get; set; }
        public Guid[] Groups { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public string UpdatedByUserId { get; internal set; }
    }
}