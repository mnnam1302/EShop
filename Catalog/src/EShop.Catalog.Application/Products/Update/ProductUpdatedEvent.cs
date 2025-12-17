using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Catalog.Application.Products.Update
{
    internal class ProductUpdatedEvent : ProductDomainEvent
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string[] Tags { get; set; } = [];
        public string Slug { get; set; } = string.Empty;
        public string[] Images { get; set; } = [];
        public Guid[] Groups { get; set; } = [];
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public string UpdatedByUserId { get; set; } = string.Empty;
    }
}