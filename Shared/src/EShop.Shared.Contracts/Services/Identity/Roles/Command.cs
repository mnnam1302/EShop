using EShop.Shared.Contracts.Abstractions.Requests;
using Newtonsoft.Json;
namespace EShop.Shared.Contracts.Services.Identity.Roles;

public static class Command
{
    public record CreateRoleCommand(string Name, string? Description, string? PhoneNumber) : ICommand;

    public record UpdateRole : ICommand
    {
        [JsonIgnore]
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string? Description { get; init; }
        public string? PhoneNumer { get; init; }
    }

    public record DeleteRole(Guid Id) : ICommand;
}