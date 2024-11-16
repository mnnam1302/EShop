using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Auth;

public static class Query
{
    public record Login : IQuery<Response.AuthenticatedResponse>
    {
        public string Username { get; init; }
        public string Password { get; init; }
    }
}