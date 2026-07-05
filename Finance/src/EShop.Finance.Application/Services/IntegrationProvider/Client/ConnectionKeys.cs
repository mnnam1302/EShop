namespace EShop.Finance.Application.Services.IntegrationProvider.Client;

public static class ConnectionKeys
{
    public const string Scheme = "Scheme";
    public const string BaseUrl = "BaseUrl";
    public const string ClientId = "ClientId";
    public const string ClientSecret = "ClientSecret";
    public const string Scope = "Scope";
    public const string IdentityServerUrl = "IdentityServerUrl";
    public const string Username = "Username";
    public const string Password = "Password";
    public const string GrantType = "GrantType";
    public const string UseClientCredentialsInHeader = "UseClientCredentialsInHeader";
}

public static class AuthenticationSchemes
{
    public const string OAuth = "OAuth";
    public const string Basic = "Basic";
    public const string NoAuth = "NoAuth";
}
