namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public sealed class UserOrganizationContextProvider : IUserOrganizationContextProvider
{
    private readonly UserOrganizationContextHttpClient _userOrganizationContextHttpClient;
    private readonly IUserOrganizationContextCachingService _userContextCachingService;
    private readonly IOrganizationContextCachingService _organizationContextCachingService;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public UserOrganizationContextProvider(
        UserOrganizationContextHttpClient userOrganizationContextHttpClient,
        IUserOrganizationContextCachingService userOrganizationContextCachingService,
        IOrganizationContextCachingService organizationContextCachingService,
        IUserDetailsProvider userDetailsProvider)
    {
        _userOrganizationContextHttpClient = userOrganizationContextHttpClient;
        _userContextCachingService = userOrganizationContextCachingService;
        _organizationContextCachingService = organizationContextCachingService;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(CancellationToken cancellationToken = default)
    {
        if (_userDetailsProvider.IsSystemUser)
        {
            return GetUserOrganizationContextForSystemUser();
        }

        var operationalUser = _userDetailsProvider.AuthenticatedUser;
        var cachedUserOrganizationContext = await _userContextCachingService.GetValue(operationalUser.Id, operationalUser.UserType, cancellationToken);

        if (cachedUserOrganizationContext != null)
        {
            return cachedUserOrganizationContext;
        }

        return await _userOrganizationContextHttpClient.GetUserOrganizationContextAsync(operationalUser.Id, cancellationToken);
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string userType = UserTypes.TenantUsers, CancellationToken cancellationToken = default)
    {
        if (_userDetailsProvider.IsSystemUser)
        {
            return GetUserOrganizationContextForSystemUser();
        }

        var cachedUserOrganizationContext = await _userContextCachingService.GetValue(userId, userType, cancellationToken);
        if (cachedUserOrganizationContext != null)
        {
            return cachedUserOrganizationContext;
        }

        return await _userOrganizationContextHttpClient.GetUserOrganizationContextAsync(userId, userType, cancellationToken);
    }

    private UserOrganizationContext GetUserOrganizationContextForSystemUser() =>
        new()
        {
            UserId = UserData.SystemUsername,
            UserDisplayName = UserData.SystemUsername,
            OrganizationId = _userDetailsProvider.AuthenticatedUser.TenantId,
            OrganizationContextPath = _userDetailsProvider.AuthenticatedUser.TenantId,
            OrganizationName = _userDetailsProvider.AuthenticatedUser.TenantId
        };

    public async Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId)
    {
        var cachedOrganizationContext = await _organizationContextCachingService.GetValue(organizationId);
        if (cachedOrganizationContext != null)
        {
            return cachedOrganizationContext;
        }

        return await _userOrganizationContextHttpClient.GetOrganizationContextForSpecificOrganizationAsync(organizationId);
    }

    public async Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath)
    {
        var cachedOrganizationContext = await _organizationContextCachingService.GetValue(organizationContextPath);
        if (cachedOrganizationContext != null)
        {
            return cachedOrganizationContext;
        }

        return await _userOrganizationContextHttpClient.GetOrganizationContextByPathAsync(organizationContextPath);
    }
}