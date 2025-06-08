using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public class UserOrganizationContextProvider : IUserOrganizationContextProvider
{
    private readonly UserOrganizationContextHttpClient _userOrganizationContextHttpClient;
    private readonly IUserOrganizationContextCachingService _userOrganizationContextCachingService;
    private readonly IOrganizationContextCachingService _organizationContextCachingService;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public UserOrganizationContextProvider(
        UserOrganizationContextHttpClient userOrganizationContextHttpClient,
        IUserOrganizationContextCachingService userOrganizationContextCachingService,
        IOrganizationContextCachingService organizationContextCachingService,
        IUserDetailsProvider userDetailsProvider)
    {
        _userOrganizationContextHttpClient = userOrganizationContextHttpClient;
        _userOrganizationContextCachingService = userOrganizationContextCachingService;
        _organizationContextCachingService = organizationContextCachingService;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync()
    {
        if (_userDetailsProvider.IsSystemUser)
        {
            return GetUserOrganizationContextForSystemUser();
        }

        var cachedUserOrganizationContext = await _userOrganizationContextCachingService
            .GetValue(_userDetailsProvider.AuthenticatedUser.Id, _userDetailsProvider.AuthenticatedUser.UserType);
        if (cachedUserOrganizationContext != null)
        {
            return cachedUserOrganizationContext;
        }

        return await _userOrganizationContextHttpClient.GetUserOrganizationContextAsync();
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = "TenantUsers")
    {
        if (_userDetailsProvider.IsSystemUser)
        {
            return GetUserOrganizationContextForSystemUser();
        }

        var cachedUserOrganizationContext = await _userOrganizationContextCachingService.GetValue(userId, typeUser);
        if (cachedUserOrganizationContext != null)
        {
            return cachedUserOrganizationContext;
        }

        return await _userOrganizationContextHttpClient.GetUserOrganizationContextAsync(userId, typeUser);
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