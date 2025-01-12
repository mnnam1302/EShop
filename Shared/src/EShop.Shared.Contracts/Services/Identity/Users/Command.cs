using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Users;

public static class Command
{
    public record RegisterUser(string UserName,
        string Password,
        string Email,
        string? DisplayName = null,
        string? PhoneNumber = null,
        DateTime? DateOfBirth = null,
        string? OrganizationId = null) : ICommand;

    public record CreateUserCommand : ICommand
    {
        public string Username { get; init; }
        public string Password { get; init; }
        public string Email { get; init; }
        public string DisplayName { get; init; }
        public string? PhoneNumber { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public List<string> RoleIds { get; init; }
        public string OrganizationId { get; init; }
    }
}