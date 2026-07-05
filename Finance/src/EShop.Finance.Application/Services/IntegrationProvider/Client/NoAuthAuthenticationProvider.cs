using EShop.Finance.Application.Services.IntegrationProvider.Models;

namespace EShop.Finance.Application.Services.IntegrationProvider.Client;

public sealed class NoAuthAuthenticationProvider : IAuthenticationProvider
{
    public string Scheme => AuthenticationSchemes.NoAuth;

    public Task Initialize(string tenantId, AuthenticationOptions options, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void ApplyAuthentication(HttpRequestMessage request)
    {
        // No authentication.
    }

    public Task<bool> VerifyAuthentication(AuthenticationOptions options, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}
