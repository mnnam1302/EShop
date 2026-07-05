using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace EShop.Finance.Application.Services.IntegrationProvider.Authentication;

public sealed class OAuthAuthenticationProvider(
    IHttpClientFactory httpClientFactory,
    IProviderSessionStore sessionStore,
    ILogger<OAuthAuthenticationProvider> logger) : IAuthenticationProvider
{
    public const string TokenClientName = "FinanceOAuthTokenClient";
    private static readonly TimeSpan ExpiryMargin = TimeSpan.FromMinutes(3);
    private const int DefaultExpiresInSeconds = 3600;

    private string? _accessToken;

    public string Scheme => AuthenticationSchemes.OAuth;

    public async Task Initialize(string tenantId, AuthenticationOptions options, CancellationToken cancellationToken)
    {
        var cached = await sessionStore.GetToken(tenantId, cancellationToken);
        if (cached is not null && cached.ExpiresAtUtc.HasValue && cached.ExpiresAtUtc.Value - ExpiryMargin > DateTimeOffset.UtcNow)
        {
            _accessToken = cached.Token;
            logger.LogDebug("Reusing cached OAuth token for tenant {TenantId}.", tenantId);
            return;
        }

        var token = await RequestToken(options, cancellationToken);
        _accessToken = token.AccessToken;

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn > 0 ? token.ExpiresIn : DefaultExpiresInSeconds);
        await sessionStore.SaveToken(tenantId, token.AccessToken, expiresAt, cancellationToken);
        logger.LogDebug("Acquired and cached a new OAuth token for tenant {TenantId}.", tenantId);
    }

    public void ApplyAuthentication(HttpRequestMessage request)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            throw new InvalidOperationException("OAuth token has not been initialised.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public async Task<bool> VerifyAuthentication(AuthenticationOptions options, CancellationToken cancellationToken)
    {
        var token = await RequestToken(options, cancellationToken);
        return !string.IsNullOrEmpty(token.AccessToken);
    }

    private async Task<TokenResponse> RequestToken(AuthenticationOptions options, CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string> { ["grant_type"] = options.GrantType };
        if (!string.IsNullOrEmpty(options.Scope))
        {
            form["scope"] = options.Scope;
        }

        if (!options.UseClientCredentialsInHeader)
        {
            if (!string.IsNullOrEmpty(options.ClientId))
                form["client_id"] = options.ClientId;

            if (!string.IsNullOrEmpty(options.ClientSecret))
                form["client_secret"] = options.ClientSecret;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, options.OauthAccessTokenEndpoint)
        {
            Content = new FormUrlEncodedContent(form),
        };

        if (options.UseClientCredentialsInHeader && !string.IsNullOrEmpty(options.ClientId))
        {
            var raw = $"{options.ClientId}:{options.ClientSecret}";
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(raw)));
        }

        var client = httpClientFactory.CreateClient(TokenClientName);
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        var token = JsonSerializer.Deserialize<TokenResponse>(body);
        if (token is null || string.IsNullOrEmpty(token.AccessToken))
        {
            throw new InvalidOperationException("The token endpoint did not return an access token.");
        }

        return token;
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }
}
