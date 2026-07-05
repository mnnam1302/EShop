namespace EShop.Finance.Application.Services.IntegrationProvider.Authentication;

public interface IAuthenticationProvider
{
    string Scheme { get; }

    Task Initialize(string tenantId, AuthenticationOptions options, CancellationToken cancellationToken);

    void ApplyAuthentication(HttpRequestMessage request);

    Task<bool> VerifyAuthentication(AuthenticationOptions options, CancellationToken cancellationToken);
}
