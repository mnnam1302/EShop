using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Tenancy.Tenants;

public static class Command
{
    public record CreateTenantCommand : ICommand
    {
        public required string Id { get; set; }

        public required string Name { get; set; }

        public required string OwnerUsername { get; set; }

        public required string OwnerEmail { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Description { get; set; }
    }

    public sealed record CreateTenantCommandInternal : ICommand
    {
        public required string TenantId { get; init; }
        public required string TenantName { get; init; }
        public required string OwnerUsername { get; init; }
        public required string OwnerDisplayName { get; init; }
        public required string OwnerEmail { get; init; }
    }
}