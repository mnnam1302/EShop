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
}