using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Catalog.ReadModels.MongoDb.Persistence;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;

namespace EShop.Catalog.ReadModels.MongoDb.Handlers;

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
    private readonly ICategoryReadRepository _categoryRepository;
    private readonly CatalogReadDbContext _dbContext;
    private readonly ILogger<UpdateCategoryProjectionCommandHandler> _logger;

    public UpdateCategoryProjectionCommandHandler(
        ICategoryReadRepository categoryRepository,
        CatalogReadDbContext dbContext,
        ILogger<UpdateCategoryProjectionCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(UpdateCategoryProjectionCommand command, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.FindSingleAsync(
            c => c.Id == command.CategoryId.ToString() && c.TenantId == command.TenantId,
            cancellationToken: cancellationToken);

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

        _categoryRepository.Update(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category projection with ID {CategoryId} for Tenant ID {TenantId} updated successfully.",
            command.CategoryId, command.TenantId);

        return Result.Success();
    }
}
