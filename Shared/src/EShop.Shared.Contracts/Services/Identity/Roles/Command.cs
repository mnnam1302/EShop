using EShop.Shared.Contracts.Abstractions.Requests;
using System.Text.Json.Serialization;

namespace EShop.Shared.Contracts.Services.Identity.Roles;

public static class Command
{
    public record CreateRole(string Name, string? Description, string? PhoneNumber) : ICommand;

    public record UpdateRole : ICommand
    {
        [JsonIgnore]
        public string Id { get; init; }

        public string Name { get; init; }
        public string? Description { get; init; }
        public string? PhoneNumer { get; init; }
    }

    public record DeleteRole(string Id) : ICommand;
}