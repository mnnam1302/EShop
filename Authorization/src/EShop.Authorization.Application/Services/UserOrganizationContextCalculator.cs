using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Authorization.Application.Services;

public interface IUserOrganizationContextCalculator
{
    Task<UserOrganizationContext> GetUserOrganizationContextAsync(CancellationToken cancellationToken);

    Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string userType, CancellationToken cancellationToken);

    Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId, CancellationToken cancellationToken);

    Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath, CancellationToken cancellationToken);
}

internal sealed class UserOrganizationContextCalculator : IUserOrganizationContextCalculator
{
    private readonly ILogger<UserOrganizationContextCalculator> _logger;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;

    public UserOrganizationContextCalculator(
        ILogger<UserOrganizationContextCalculator> logger,
        IUserDetailsProvider userDetailsProvider,
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository)
    {
        _logger = logger;
        _userDetailsProvider = userDetailsProvider;
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(CancellationToken cancellationToken)
    {
        try
        {
            var authenticatedUser = _userDetailsProvider.AuthenticatedUser;
            _userDetailsProvider.SetSystemUserContext(authenticatedUser.TenantId);

            var userOrganizationContext = await CalculateUserOrganizationContext(
                authenticatedUser.Id,
                authenticatedUser.UserType,
                cancellationToken);

            if (userOrganizationContext == null)
            {
                throw new NotFoundException($"User organization context '{authenticatedUser.Id}' is not found.");
            }

            return userOrganizationContext;
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task<UserOrganizationContext?> CalculateUserOrganizationContext(string userId, string userType, CancellationToken cancellationToken)
    {
        return userType switch
        {
            UserTypes.SystemUsers => await GetTenantUserOrganizationContextAsync(userId, cancellationToken),
            _ => await GetTenantUserOrganizationContextAsync(userId, cancellationToken)
        };
    }

    private async Task<UserOrganizationContext?> GetTenantUserOrganizationContextAsync(string userId, CancellationToken cancellationToken)
    {
        var userOrganization = await _userRepository.FindByCondition(
                predicate: u => u.Id == userId && u.Organization != null,
                trackChanges: false,
                includeProperties: u => u.Organization!)
            .Select(u => new UserOrganizationContext
            {
                OrganizationId = u.Organization!.Id,
                OrganizationContextPath = u.Organization!.Context.Path, // important Ring-fencing
                OrganizationName = u.Organization.Name,
                OrganizationNumber = u.Organization.OrganizationNumber,
                OrganizationPhoneNumber = u.Organization.PhoneNumber,
                OrganizationEmail = u.Organization.Email,
                OrganizationStreet = u.Organization.Address.Street,
                OrganizationCity = u.Organization.Address.City,
                OrganizationCountry = u.Organization.Address.Country,
                UserId = u.Id,
                UserDisplayName = u.Name,
                UserEmail = u.Email,
                UserPhoneNumber = u.PhoneNumber,
            })
            .SingleOrDefaultAsync(cancellationToken);

        return userOrganization;
    }

    public Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
