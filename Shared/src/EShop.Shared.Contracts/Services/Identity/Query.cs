using EShop.Shared.Contracts.Abstractions.Requests;
using MassTransit;

namespace EShop.Shared.Contracts.Services.Identity;

public static class Query
{
    public record Login : IQuery<Response.AuthenticatedResponse>
    {
        public string Username { get; init; }
        public string Password { get; init; }
    }

    public record GetRoles() : IQuery<List<Response.RolesResponse>>;
}