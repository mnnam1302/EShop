using EShop.Catalog.SyncService.MongoDb.Entities;
using EShop.Catalog.SyncService.MongoDb.Infrastructure;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;

namespace EShop.Catalog.SyncService.MongoDb.UseCases.Category;

public sealed class UpdateCategoryProjectionCommand : ICommand
{
    public Guid CategoryId { get; set; }
    public ulong Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public required string TenantId { get; set; }
}

public sealed class UpdateCategoryProjectionCommandHandler : ICommandHandler<UpdateCategoryProjectionCommand>
{
    private readonly IMongoRepository<CategoryProjection> _mongoRepository;
    private readonly ILogger<UpdateCategoryProjectionCommandHandler> _logger;

    public UpdateCategoryProjectionCommandHandler(
        IMongoRepository<CategoryProjection> mongoRepository,
        ILogger<UpdateCategoryProjectionCommandHandler> logger)
    {
        _mongoRepository = mongoRepository;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(UpdateCategoryProjectionCommand command, CancellationToken cancellationToken)
    {
        var category = await _mongoRepository.FindOneAsync(
            filterExpression: c => c.DocumentId == command.CategoryId && c.TenantId == command.TenantId,
            cancellationToken);

        if (category is null)
        {
            _logger.LogWarning("Category projection with ID {CategoryId} and Tenant ID {TenantId} not found for update.",
                command.CategoryId, command.TenantId);

            return Result.Failure(new Error("CategoryProjection.NotFound", $"The category projection {command.CategoryId} was not found."));
        }

        category.Version = command.Version;
        category.Name = command.Name;
        category.Reference = command.Reference;
        category.Slug = command.Slug;
        category.ParentId = command.ParentId;
        category.CreatedAtUtc = command.CreatedAtUtc;
        category.UpdatedAtUtc = command.UpdatedAtUtc;

        await _mongoRepository.ReplaceOneAsync(category, cancellationToken);

        _logger.LogInformation("Category projection with ID {CategoryId} for Tenant ID {TenantId} updated successfully.",
            command.CategoryId, command.TenantId);

        return Result.Success();
    }
}
