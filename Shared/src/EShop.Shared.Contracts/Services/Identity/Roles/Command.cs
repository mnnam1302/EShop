using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Roles;

public static class Command
{
    public record CreateRole : ICommand
    {
        public string Name { get; init; }
        public string? Description { get; init; }
        public string? PhoneNumer { get; init; }
    }
}