using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Users;

public static class Command
{
    public record RegisterUser(string UserName,
        string Password,
        string Email,
        string? DisplayName,
        string? PhoneNumber,
        DateTime? DateOfBirth,
        string? OrganizationId) : ICommand;
}