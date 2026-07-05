namespace EShop.Finance.Application.Services.IntegrationProvider.Client;

public interface IAuthenticationProviderResolver
{
    IAuthenticationProvider Resolve(string scheme);
}


internal sealed class AuthenticationProviderResolver : IAuthenticationProviderResolver
{
    private readonly IEnumerable<IAuthenticationProvider> providers;

    public AuthenticationProviderResolver(IEnumerable<IAuthenticationProvider> providers)
    {
        this.providers = providers;
    }

    public IAuthenticationProvider Resolve(string scheme)
    {
        var provider = providers.SingleOrDefault(p => p.Scheme.Equals(scheme, StringComparison.OrdinalIgnoreCase));
        if (provider is null)
        {
            throw new InvalidOperationException($"No authentication provider is registered for scheme '{scheme}'.");
        }

        return provider;
    }
}
