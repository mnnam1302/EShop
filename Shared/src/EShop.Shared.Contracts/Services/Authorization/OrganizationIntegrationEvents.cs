using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Authorization;

public sealed class OrganizationCreated : IntegrationEvent
{
    public required string OrganizationId { get; init; }
    public required string Name { get; init; }
    public string? ParentOrganizationId { get; init; }
}
