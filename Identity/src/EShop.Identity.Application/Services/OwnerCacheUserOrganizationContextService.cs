using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Identity.Application.Services;

public class OwnerCacheUserOrganizationContextService : IUserOrganizationContextProvider
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IOrganizationContextCachingService _organizationContextCachingService;
    private readonly IUserOrganizationContextCachingService _userOrganizationContextCachingService;
    private readonly IUserOrganizationContextCalculator _userOrganizationContextCalculator;

    public OwnerCacheUserOrganizationContextService(
        IUserDetailsProvider userDetailsProvider,
        IOrganizationContextCachingService organizationContextCachingService,
        IUserOrganizationContextCachingService userOrganizationContextCachingService,
        IUserOrganizationContextCalculator userOrganizationContextCalculator)
    {
        _userDetailsProvider = userDetailsProvider;
        _organizationContextCachingService = organizationContextCachingService;
        _userOrganizationContextCachingService = userOrganizationContextCachingService;
        _userOrganizationContextCalculator = userOrganizationContextCalculator;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync()
    {
        var authenticatedUser = _userDetailsProvider.AuthenticatedUser;

        if (UserData.IsSystemUser(authenticatedUser.UserType))
        {
            return GetUserOrganizationContextForSystemUser();
        }

        var cachedUserOrganizationContext = await _userOrganizationContextCachingService.GetValue(
            authenticatedUser.Id,
            authenticatedUser.UserType);

        if (cachedUserOrganizationContext != null)
        {
            return cachedUserOrganizationContext;
        }

        var userOrganizationContext = await _userOrganizationContextCalculator.GetUserOrganizationContextAsync();

        await _userOrganizationContextCachingService.AddValue(
            authenticatedUser.Id,
            authenticatedUser.UserType,
            userOrganizationContext);

        return userOrganizationContext;
    }

    public UserOrganizationContext GetUserOrganizationContextForSystemUser()
    {
        return new UserOrganizationContext
        {
            OrganizationId = _userDetailsProvider.AuthenticatedUser.TenantId,
            OrganizationName = _userDetailsProvider.AuthenticatedUser.TenantId,
            OrganizationContextPath = _userDetailsProvider.AuthenticatedUser.TenantId,
            UserId = UserData.SystemUsername,
            UserDisplayName = UserData.SystemUsername,
        };
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = UserTypes.TenantUsers)
    {
        var cachedUserOrganizationContext = await _userOrganizationContextCachingService.GetValue(userId, typeUser);
        if (cachedUserOrganizationContext != null)
        {
            return cachedUserOrganizationContext;
        }
        var userOrganizationContext = await _userOrganizationContextCalculator.GetUserOrganizationContextForSpecificUserAsync(userId, typeUser);
        await _userOrganizationContextCachingService.AddValue(userId, typeUser, userOrganizationContext);

        return userOrganizationContext;
    }

    public async Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId)
    {
        if (string.IsNullOrEmpty(organizationId))
        {
            throw new ArgumentException("Organization ID cannot be null or empty", nameof(organizationId));
        }

        var cachedOrganizationContext = await _organizationContextCachingService.GetValue(organizationId);
        if (cachedOrganizationContext != null)
        {
            return cachedOrganizationContext;
        }

        var organizationContext = await _userOrganizationContextCalculator.GetOrganizationContextForSpecificOrganizationAsync(organizationId);
        await _organizationContextCachingService.AddValue(organizationId, organizationContext);

        return organizationContext;
    }

    public async Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath)
    {
        if (string.IsNullOrEmpty(organizationContextPath))
        {
            throw new ArgumentException("Organization ID cannot be null or empty", nameof(organizationContextPath));
        }

        var cachedOrganizationContext = await _organizationContextCachingService.GetValue(organizationContextPath);
        if (cachedOrganizationContext != null)
        {
            return cachedOrganizationContext;
        }

        var organizationContext = await _userOrganizationContextCalculator.GetOrganizationContextByPathAsync(organizationContextPath);
        await _organizationContextCachingService.AddValue(organizationContextPath, organizationContext);

        return organizationContext;
    }
}