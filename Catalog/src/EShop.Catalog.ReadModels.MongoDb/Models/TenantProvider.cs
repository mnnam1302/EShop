using EShop.Shared.Authentication.Abstractions;

namespace EShop.Catalog.ReadModels.MongoDb.Models;

public sealed class TenantProvider : ITenantProvider
{
    private readonly IUserDetailsProvider _userDetailsProvider;

    public TenantProvider(IUserDetailsProvider userDetailsProvider)
    {
        _userDetailsProvider = userDetailsProvider;
    }

    public string? TenantId => _userDetailsProvider.IsAuthenticatedUser
        ? _userDetailsProvider.AuthenticatedUser.TenantId
        : null;
}
