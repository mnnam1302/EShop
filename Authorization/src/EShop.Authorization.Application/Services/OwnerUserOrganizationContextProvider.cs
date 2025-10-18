using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

namespace EShop.Authorization.Application.Services;

internal sealed class OwnerUserOrganizationContextProvider : IUserOrganizationContextProvider
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IUserOrganizationContextCalculator _userContextCalculator;
    private readonly IUserOrganizationContextCachingService _userContextCachingService;
    private readonly IOrganizationContextCachingService _organizationContextCachingService;

    public OwnerUserOrganizationContextProvider(
        IUserDetailsProvider userDetailsProvider,
        IUserOrganizationContextCalculator userContextCalculator,
        IUserOrganizationContextCachingService userOrganizationContextCachingService,
        IOrganizationContextCachingService organizationContextCachingService)
    {
        _userDetailsProvider = userDetailsProvider;
        _userContextCalculator = userContextCalculator;
        _userContextCachingService = userOrganizationContextCachingService;
        _organizationContextCachingService = organizationContextCachingService;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(CancellationToken cancellationToken = default)
    {
        var operationalUser = _userDetailsProvider.AuthenticatedUser;

        if (UserData.IsSystemUser(operationalUser.Id))
        {
            return GetUserOrganizationContextForSystemUser();
        }

        var cachedValue = await _userContextCachingService.GetValue(operationalUser.Id, operationalUser.UserType, cancellationToken);

        if (cachedValue != null)
        {
            return cachedValue;
        }

        var userOrganizationContext = await _userContextCalculator.GetUserOrganizationContextAsync(cancellationToken);

        await _userContextCachingService.AddValue(
            operationalUser.Id,
            operationalUser.UserType,
            userOrganizationContext,
            cancellationToken);

        return userOrganizationContext;
    }

    public UserOrganizationContext GetUserOrganizationContextForSystemUser()
    {
        return new UserOrganizationContext
        {
            UserId = UserData.SystemUsername,
            UserDisplayName = UserData.SystemUsername,
            OrganizationId = _userDetailsProvider.AuthenticatedUser.TenantId,
            OrganizationName = _userDetailsProvider.AuthenticatedUser.TenantId,
            OrganizationContextPath = _userDetailsProvider.AuthenticatedUser.TenantId,
        };
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string userType = UserTypes.TenantUsers, CancellationToken cancellationToken = default)
    {
        var cachedUserOrganizationContext = await _userContextCachingService.GetValue(userId, userType, cancellationToken);

        if (cachedUserOrganizationContext != null)
        {
            return cachedUserOrganizationContext;
        }

        var userOrganizationContext = await _userContextCalculator.GetUserOrganizationContextForSpecificUserAsync(userId, userType, cancellationToken);

        await _userContextCachingService.AddValue(userId, userType, userOrganizationContext);

        return userOrganizationContext;
    }

    public async Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(organizationId))
        {
            throw new ArgumentException("Organization ID must not null or empty.", nameof(organizationId));
        }

        var cachedOrganizationContext = await _organizationContextCachingService.GetValue(organizationId, cancellationToken);

        if (cachedOrganizationContext != null)
        {
            return cachedOrganizationContext;
        }

        var organizationContext = await _userContextCalculator.GetOrganizationContextForSpecificOrganizationAsync(organizationId, cancellationToken);

        await _organizationContextCachingService.AddValue(organizationId, organizationContext, cancellationToken);

        return organizationContext;
    }

    public async Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(organizationContextPath))
        {
            throw new ArgumentException("Organization context path must not null or empty.", nameof(organizationContextPath));
        }

        var cachedOrganizationContext = await _organizationContextCachingService.GetValue(organizationContextPath, cancellationToken);

        if (cachedOrganizationContext != null)
        {
            return cachedOrganizationContext;
        }

        var organizationContext = await _userContextCalculator.GetOrganizationContextByPathAsync(organizationContextPath, cancellationToken);

        await _organizationContextCachingService.AddValue(organizationContextPath, organizationContext);

        return organizationContext;
    }
}
