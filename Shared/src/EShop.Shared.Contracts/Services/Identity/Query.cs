using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity;

public static class Query
{
    public record Login : IQuery<Response.AuthenticatedResponse>
    {
        public string UserName { get; init; }
        public string Password { get; init; }
    }
}