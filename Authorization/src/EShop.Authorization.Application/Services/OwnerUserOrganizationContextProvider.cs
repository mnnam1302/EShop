using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

namespace EShop.Authorization.Application.Services
{
    internal sealed class OwnerUserOrganizationContextProvider : IUserOrganizationContextProvider
    {
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly IUserOrganizationContextCalculator _userContextCalculator;
        private readonly IUserOrganizationContextCachingService _userContextCachingService;

        public OwnerUserOrganizationContextProvider(
            IUserDetailsProvider userDetailsProvider,
            IUserOrganizationContextCalculator userContextCalculator,
            IUserOrganizationContextCachingService userOrganizationContextCachingService)
        {
            _userDetailsProvider = userDetailsProvider;
            _userContextCalculator = userContextCalculator;
            _userContextCachingService = userOrganizationContextCachingService;
        }

        public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(CancellationToken cancellationToken = default)
        {
            var user = _userDetailsProvider.AuthenticatedUser;

            if (UserData.IsSystemUser(user.UserType))
            {
                return GetUserOrganizationContextForSystemUser();
            }

            var cachedValue = await _userContextCachingService.GetValue(user.Id, user.UserType);

            if (cachedValue != null)
            {
                return cachedValue;
            }

            var userOrganizationContext = await _userContextCalculator.GetUserOrganizationContextAsync(cancellationToken);

            await _userContextCachingService.AddValue(
                user.Id,
                user.UserType,
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

        public Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath)
        {
            throw new NotImplementedException();
        }

        public Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId)
        {
            throw new NotImplementedException();
        }
    }
}
