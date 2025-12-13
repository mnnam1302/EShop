using EShop.Catalog.SyncService.MongoDb.Infrastructure;
using EShop.Catalog.SyncService.MongoDb.Models;
using EShop.Catalog.SyncService.MongoDb.UseCases.Category;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS;

namespace EShop.Catalog.SyncService.MongoDb.Consumers;

public sealed class CategoryUpdatedConsumer : IdempotentConsumer<CategoryUpdated>
{
    private readonly IMediator mediator;

    public CategoryUpdatedConsumer(IMongoRepository<InboxMessageProjection> mongoRepository, IMediator mediator) : base(mongoRepository)
    {
        this.mediator = mediator;
    }

    protected override async Task<Result> HandleMessageAsync(CategoryUpdated message, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryProjectionCommand
        {
            CategoryId = message.CategoryId,
            Version = message.Version,
            Name = message.Name,
            Reference = message.Reference,
            Slug = message.Slug,
            ParentId = message.ParentId,
            CreatedAtUtc = message.CreatedAtUtc,
            UpdatedAtUtc = message.UpdatedAtUtc,
            TenantId = message.TenantId
        };

        return await mediator.SendAsync(command, cancellationToken);
    }
}
