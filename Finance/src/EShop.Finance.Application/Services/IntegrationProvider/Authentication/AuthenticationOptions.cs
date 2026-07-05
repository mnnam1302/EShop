namespace EShop.Finance.Application.Services.IntegrationProvider.Authentication;

public sealed class AuthenticationOptions
{
    public string Scheme { get; private init; } = AuthenticationSchemes.OAuth;
    public string? BaseUrl { get; private init; }
    public string? ClientId { get; private init; }
    public string? ClientSecret { get; private init; }
    public string? Scope { get; private init; }
    public string? OauthAccessTokenEndpoint { get; private init; }
    public string? Username { get; private init; }
    public string? Password { get; private init; }
    public string GrantType { get; private init; } = "client_credentials";
    public bool UseClientCredentialsInHeader { get; private init; }

    public IReadOnlyDictionary<string, string?> ConnectionDetails { get; private init; } = new Dictionary<string, string?>();

    public static AuthenticationOptions Create(IReadOnlyDictionary<string, string?> connectionDetails)
    {
        var options = new AuthenticationOptions
        {
            Scheme = Get(connectionDetails, ConnectionKeys.Scheme) ?? AuthenticationSchemes.OAuth,
            BaseUrl = Get(connectionDetails, ConnectionKeys.BaseUrl),
            ClientId = Get(connectionDetails, ConnectionKeys.ClientId),
            ClientSecret = Get(connectionDetails, ConnectionKeys.ClientSecret),
            Scope = Get(connectionDetails, ConnectionKeys.Scope),
            OauthAccessTokenEndpoint = Get(connectionDetails, ConnectionKeys.IdentityServerUrl),
            Username = Get(connectionDetails, ConnectionKeys.Username),
            Password = Get(connectionDetails, ConnectionKeys.Password),
            GrantType = Get(connectionDetails, ConnectionKeys.GrantType) ?? "client_credentials",
            UseClientCredentialsInHeader = bool.TryParse(Get(connectionDetails, ConnectionKeys.UseClientCredentialsInHeader), out var flag) && flag,
            ConnectionDetails = connectionDetails,
        };

        options.Validate();
        return options;
    }

    private void Validate()
    {
        if (Scheme.Equals(AuthenticationSchemes.OAuth, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(OauthAccessTokenEndpoint) ||
                string.IsNullOrEmpty(ClientId) ||
                string.IsNullOrEmpty(ClientSecret) ||
                string.IsNullOrEmpty(Scope))
            {
                throw new InvalidOperationException("OAuth requires IdentityServerUrl, ClientId, ClientSecret and Scope.");
            }
        }
        else if (Scheme.Equals(AuthenticationSchemes.Basic, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password))
            {
                throw new InvalidOperationException("Basic authentication requires a Username or Password.");
            }
        }
    }

    private static string? Get(IReadOnlyDictionary<string, string?> details, string key)
    {
        foreach (var pair in details)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }
}
