using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Shared;
using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Contracts.Services.Tenancy.Tenants;

public static class Command
{
    public record CreateTenantCommand : ICommand
    {
        [MaxLength(ModelConstants.ShortText)]
        public required string Id { get; set; }

        [MaxLength(ModelConstants.ShortMediumText)]
        public required string Name { get; set; }

        [MaxLength(ModelConstants.MediumText)]
        public required string OwnerUsername { get; set; }

        [MaxLength(ModelConstants.MediumLongText)]
        public required string Email { get; set; }

        public required string PhoneNumber { get; set; }

        [MaxLength(ModelConstants.LongText)]
        public string? Description { get; set; }
    }

    public record CreateTenantCommandInternal(string TenantId, string TenantName, string OwnerUsername, string OwnerDisplayName, string OwnerEmail) : ICommand;
}