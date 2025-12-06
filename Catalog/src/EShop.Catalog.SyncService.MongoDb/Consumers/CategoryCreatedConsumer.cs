using EShop.Catalog.SyncService.MongoDb.Abstractions;
using EShop.Catalog.SyncService.MongoDb.Entities;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;

namespace EShop.Catalog.SyncService.MongoDb.Consumers;

public sealed class CategoryCreatedConsumer : IdempotentConsumer<CategoryCreated>
{
    public CategoryCreatedConsumer(IMongoRepository<InboxMessageProjection> mongoRepository)
        : base(mongoRepository)
    {
    }

    protected override Task<Result> HandleMessageAsync(CategoryCreated message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}