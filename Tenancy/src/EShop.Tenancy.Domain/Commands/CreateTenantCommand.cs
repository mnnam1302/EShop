using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Tenancy.Domain.Commands;

public sealed class CreateTenantCommand : ICommand
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string OwnerUsername { get; set; }
    public required string OwnerEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
}
