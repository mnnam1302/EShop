using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Shared;
using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Contracts.Services.Identity.Users;

public static class Command
{
    public sealed record RegisterUser(
        string Username,
        string Password,
        string Email,
        string DisplayName,
        string? PhoneNumber = null,
        DateTime? DateOfBirth = null,
        string? OrganizationId = null) : ICommand;

    public sealed record CreateUserCommand : ICommand
    {
        [MaxLength(ModelConstants.MediumText)]
        public required string Username { get; init; }

        [MaxLength(ModelConstants.MediumText)]
        [EmailAddress]
        public required string Email { get; init; }

        [MaxLength(ModelConstants.MediumText)]
        public required string DisplayName { get; init; }

        [MaxLength(ModelConstants.LongText)]
        public string? PhoneNumber { get; init; }

        [MaxLength(ModelConstants.ShortText)]
        public required string OrganizationId { get; init; }

        public required Guid[] RoleIds { get; init; } = [];
    }
}