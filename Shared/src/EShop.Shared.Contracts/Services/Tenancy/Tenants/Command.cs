using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Shared;
using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Contracts.Services.Tenancy.Tenants;

public static class Command
{
    public record CreateTenantCommand : ICommand
    {
        [MaxLength(ModelConstants.ShortMediumText)]
        public required string Name { get; init; }

        [MaxLength(ModelConstants.MediumText)]
        public required string OwnerUsername { get; init; }

        [MaxLength(ModelConstants.MediumLongText)]
        public required string Email { get; init; }

        public required string PhoneNumber { get; init; }

        [MaxLength(ModelConstants.LongText)]
        public string? Description { get; init; }
    }
}