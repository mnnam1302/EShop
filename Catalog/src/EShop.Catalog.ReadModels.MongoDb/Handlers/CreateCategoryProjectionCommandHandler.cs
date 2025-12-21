using EShop.Catalog.ReadModels.MongoDb.Infrastructure;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;

namespace EShop.Catalog.ReadModels.MongoDb.Handlers;

public sealed class CreateCategoryProjectionCommand : ICommand
{
    public required Guid CategoryId { get; init; }
    public ulong Version { get; init; }
    public required string Name { get; init; }
    public required string Reference { get; init; }
    public required string Slug { get; init; }
    public Guid? ParentId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; init; }
    public required string TenantId { get; init; }
}

public sealed class CreateCategoryProjectionCommandHandler : ICommandHandler<CreateCategoryProjectionCommand>
{
    private readonly IMongoRepositoryBase<Models.Category> _mongoRepository;
    private readonly ILogger<CreateCategoryProjectionCommandHandler> _logger;

    public CreateCategoryProjectionCommandHandler(
        IMongoRepositoryBase<Models.Category> mongoRepository,
        ILogger<CreateCategoryProjectionCommandHandler> logger)
    {
        _mongoRepository = mongoRepository;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(CreateCategoryProjectionCommand command, CancellationToken cancellationToken)
    {
        var categoryProjection = await _mongoRepository.FindOneAsync(cp => cp.DocumentId == command.CategoryId, cancellationToken: cancellationToken);

        if (categoryProjection is not null)
        {
            return Result.Failure(new Error("CategoryProjectionAlreadyExists", $"Category projection with ID '{command.CategoryId}' already exists."));
        }

        _logger.LogInformation("Creating category projection with ID '{CategoryId}'", command.CategoryId);

        categoryProjection = new Models.Category
        {
            DocumentId = command.CategoryId,
            Version = command.Version,
            Name = command.Name,
            Reference = command.Reference,
            Slug = command.Slug,
            ParentId = command.ParentId,
            CreatedAtUtc = command.CreatedAtUtc,
            UpdatedAtUtc = command.UpdatedAtUtc,
            TenantId = command.TenantId,
        };

        await _mongoRepository.InsertOneAsync(categoryProjection, cancellationToken);

        _logger.LogInformation("Category projection with ID '{CategoryId}' created successfully", command.CategoryId);

        return Result.Success();
    }
}
