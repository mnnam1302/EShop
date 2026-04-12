using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Catalog.ReadModels.MongoDb.Persistence;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.ReadModel;

namespace EShop.Catalog.ReadModels.MongoDb.Consumers;

public sealed class VariantCreatedConsumer : IdempotentConsumer<VariantCreated>
{
    private readonly IReadModelProjector<Product> _projector;

    public VariantCreatedConsumer(CatalogReadDbContext dbContext, IReadModelProjector<Product> projector)
        : base(dbContext)
    {
        _projector = projector;
    }

    protected override async Task<Result> HandleMessageAsync(VariantCreated message, CancellationToken cancellationToken)
    {
        await _projector.ProjectAsync(message, cancellationToken);
        return Result.Success();
    }
}
