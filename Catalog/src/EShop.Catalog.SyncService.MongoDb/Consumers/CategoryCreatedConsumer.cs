using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.EventBus.Abstractions;
using MassTransit;

namespace EShop.Catalog.SyncService.MongoDb.Consumers;

public sealed class CategoryCreatedConsumer : IConsumer<CategoryCreated>
{
    public Task Consume(ConsumeContext<CategoryCreated> context)
    {
        throw new NotImplementedException();
    }
}
