using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.EntityFrameworkCore;

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
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IUserRepository _userRepository;

    public UserOrganizationContextCalculator(IUserDetailsProvider userDetailsProvider, IUserRepository userRepository)
    {
        _userDetailsProvider = userDetailsProvider;
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

    public async Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string userType, CancellationToken cancellationToken)
    {
        try
        {
            var operationalUser = _userDetailsProvider.AuthenticatedUser;
            _userDetailsProvider.SetSystemUserContext(operationalUser.TenantId);

            var userOrganizationContext = await CalculateUserOrganizationContext(userId, userType, cancellationToken);

            if (userOrganizationContext == null)
            {
                throw new NotFoundException($"User organization context with Id {userId} is not found");
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

    public Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
