using EShop.Catalog.ReadModels.MongoDb.Handlers;
using EShop.Catalog.ReadModels.MongoDb.Infrastructure;
using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS;

namespace EShop.Catalog.ReadModels.MongoDb.Consumers;

public sealed class CategoryCreatedConsumer : IdempotentConsumer<CategoryCreated>
{
    private readonly IMediator mediator;

    public CategoryCreatedConsumer(IMongoRepositoryBase<InboxMessage> mongoRepository, IMediator mediator)
        : base(mongoRepository)
    {
        this.mediator = mediator;
    }

    protected override Task<Result> HandleMessageAsync(CategoryCreated message, CancellationToken cancellationToken)
    {
        var command = new CreateCategoryProjectionCommand
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

        return mediator.SendAsync(command, cancellationToken);
    }
}