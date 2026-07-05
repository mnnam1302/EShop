namespace EShop.Finance.Application.Services.IntegrationProvider.Authentication;

/// <summary>
/// An authentication scheme (OAuth / Basic / NoAuth) applied to outgoing integration requests.
/// </summary>
public interface IAuthenticationProvider
{
    /// <summary>
    /// The scheme this provider handles (see <see cref="AuthenticationSchemes"/>).
    /// </summary>
    string Scheme { get; }

    /// <summary>Prepares authentication for the tenant (e.g. acquires/reuses an OAuth token).</summary>
    Task Initialize(string tenantId, AuthenticationOptions options, CancellationToken cancellationToken);

    void ApplyAuthentication(HttpRequestMessage request);

    Task<bool> VerifyAuthentication(AuthenticationOptions options, CancellationToken cancellationToken);
}
