//using EShop.Authorization.Domain.Repositories;
//using EShop.Shared.Scoping;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;
//using static EShop.Shared.Contracts.Services.Identity.Users.Response;

//namespace EShop.Authorization.Application.Services;

//public interface IUserOrganizationContextCalculator
//{
//    Task<UserOrganizationContext> GetUserOrganizationContextAsync(CancellationToken cancellationToken);

//    Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = UserTypes.TenantUsers, CancellationToken cancellationToken);

//    Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId, CancellationToken cancellationToken);

//    Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath, CancellationToken cancellationToken);
//}

//internal sealed class UserOrganizationContextCalculator : IUserOrganizationContextCalculator
//{
//    private readonly ILogger<UserOrganizationContextCalculator> _logger;
//    private readonly IUserDetailsProvider _userDetailsProvider;
//    private readonly IOrganizationRepository _organizationRepository;
//    private readonly IUserRepository _userRepository;

//    public UserOrganizationContextCalculator(
//        ILogger<UserOrganizationContextCalculator> logger,
//        IUserDetailsProvider userDetailsProvider,
//        IOrganizationRepository organizationRepository,
//        IUserRepository userRepository)
//    {
//        _logger = logger;
//        _userDetailsProvider = userDetailsProvider;
//        _organizationRepository = organizationRepository;
//        _userRepository = userRepository;
//    }

//    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(CancellationToken cancellationToken)
//    {
//        try
//        {
//            var authenticatedUser = _userDetailsProvider.AuthenticatedUser;
//            _userDetailsProvider.SetSystemUserContext(authenticatedUser.TenantId);

//            //var userOrganizationContext = await CalculateUserOrganizationContextInternal(authenticatedUser.Id, authenticatedUser.UserType, authenticatedUser.TenantId);

//            //if (userOrganizationContext == null)
//            //{
//            //throw new NotFoundException("User organization context is not found");
//            //}

//            //return userOrganizationContext;

//            return null;
//        }
//        finally
//        {
//            _userDetailsProvider.ClearSystemUserContext();
//        }
//    }

//    private async Task<UserOrganizationContext?> CalculateUserOrganizationContextInternal(string userId, string userType, string tenantId, CancellationToken cancellationToken)
//    {
//        return userType switch
//        {
//            UserTypes.SystemUsers => await GetTenantUserOrganizationContextAsync(userId),
//            _ => await GetTenantUserOrganizationContextAsync(userId)
//        };
//    }
//    private async Task<UserOrganizationContext?> GetTenantUserOrganizationContextAsync(string userId)
//    {
//        var userOrganization = await _userRepository
//            .FindByCondition(
//                predicate: u => u.Id == userId && u.Organization != null,
//                trackChanges: false,
//                includeProperties: u => u.Organization!)
//            .Select(u => new UserOrganizationContext
//            {
//                OrganizationId = u.Organization!.Id,
//                //OrganizationContextPath = u.Organization!.Context.Path, // important Ring-fencing
//                OrganizationName = u.Organization.Name,
//                //OrganizationNumber = u.Organization.OrganizationNumber,
//                //OrganizationPhoneNumber = u.Organization.PhoneNumber,
//                //OrganizationEmail = u.Organization.Email,
//                //OrganizationAddress = u.Organization.Address,
//                //OrganizationCity = u.Organization.City,
//                //OrganizationPostcode = u.Organization.Postcode,
//                UserId = u.Id,
//                //UserDisplayName = u.DisplayName,
//                //UserEmail = u.Email,
//                //UserPhoneNumber = u.PhoneNumber,
//            })
//            .SingleOrDefaultAsync();

//        if (userOrganization == null)
//        {
//            _logger.LogInformation("The tenant user with id '{userId}' was not found", userId);
//            return null;
//        }

//        _logger.LogDebug(
//                "The tenant user organization context path '{orgContextPath}' was retrieved for userId '{id}', userType '{userType}'",
//                userOrganization.OrganizationContextPath,
//                userId,
//                UserTypes.TenantUsers);

//        return userOrganization;
//    }

//    public Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = "TenantUsers", CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }
//}
