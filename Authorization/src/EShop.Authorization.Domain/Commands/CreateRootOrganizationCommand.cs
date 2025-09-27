using EShop.Shared.CQRS.Command;

namespace EShop.Authorization.Domain.Commands;

public sealed class CreateRootOrganizationCommand : ICommand
{
    public required string TenantId { get; init; }
    public required string TenantName { get; init; }
    public required string OwnerUsername { get; init; }
    public required string OwnerDisplayName { get; init; }
    public required string OwnerEmail { get; init; }
}
