using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Shared;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Contracts.Services.Identity.Organizations;

public static class Command
{
    public sealed record CreateOrganizationCommand : ICommand
    {
        [MaxLength(ModelConstants.ShortText)]
        public required string Id { get; init; }

        [MaxLength(ModelConstants.MediumText)]
        public required string Name { get; init; }

        [MaxLength(ModelConstants.MediumText)]
        public required string Email { get; init; }

        [MaxLength(ModelConstants.ShortText)]
        public string? OrganizationNumber { get; init; }

        [MaxLength(ModelConstants.ShortText)]
        public string? PhoneNumber { get; init; }

        [MaxLength(ModelConstants.LongText)]
        public string? Address { get; init; }

        [MaxLength(ModelConstants.MediumText)]
        public string? City { get; init; }

        [MaxLength(ModelConstants.TinyText)]
        public string? PostCode { get; init; }

        [MaxLength(ModelConstants.LongText)]
        public string? Description { get; init; }

        [MaxLength(ModelConstants.ShortText)]
        public required string ParentOrganizationId { get; init; }
    }

    public sealed record UpdateOrganizationCommand : ICommand
    {
        [JsonIgnore]
        public string? Id { get; init; }
        public required string Name { get; init; }
        public string? OrganizationNumber { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? PostCode { get; init; }
        public string? Description { get; init; }
        public string? ParentOrganizationId { get; init; }
    }
}