using EShop.Inventory.Domain.Abstractions;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Application.UseCases.Inventory;

public sealed class CreateInventoryCommand : ICommand
{
    public required Guid ProductId { get; init; }
    public required Guid VariantId { get; init; }
    public required string Sku { get; init; }
    public required int StockAvailable { get; init; }
    public int MinimumStock { get; init; }
}

internal sealed class CreateInventoryCommandHandler : ICommandHandler<CreateInventoryCommand>
{
    private readonly IInventoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ILogger<CreateInventoryCommandHandler> _logger;

    public CreateInventoryCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork,
        IUserDetailsProvider userDetailsProvider,
        ILogger<CreateInventoryCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _userDetailsProvider = userDetailsProvider;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(CreateInventoryCommand command, CancellationToken cancellationToken)
    {
        var existing = await _repository.FindSingleAsync(
            x => x.VariantId == command.VariantId,
            cancellationToken: cancellationToken);

        if (existing is not null)
        {
            return Result.Failure(new Error("Inventory.AlreadyExists", $"Inventory for SKU '{command.Sku}' already exists."));
        }

        var currentUser = _userDetailsProvider.AuthenticatedUser;

        var inventory = Domain.Entities.Inventory.Create(
            command.ProductId,
            command.VariantId,
            command.Sku,
            command.StockAvailable,
            command.MinimumStock,
            currentUser);

        _repository.Add(inventory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory created for SKU {Sku} (SkuId: {SkuId})", inventory.Sku, inventory.VariantId);

        return Result.Success();
    }
}
