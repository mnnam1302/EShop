using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

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
    private readonly IOrganizationRepository _organizationRepository;

    public UserOrganizationContextCalculator(
        IUserDetailsProvider userDetailsProvider,
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository)
    {
        _userDetailsProvider = userDetailsProvider;
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
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

            return userOrganizationContext;
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task<UserOrganizationContext> CalculateUserOrganizationContext(string userId, string userType, CancellationToken cancellationToken)
    {
        return userType switch
        {
            UserTypes.SystemUsers => await GetTenantUserOrganizationContextAsync(userId, cancellationToken),
            _ => await GetTenantUserOrganizationContextAsync(userId, cancellationToken)
        };
    }

    private async Task<UserOrganizationContext> GetTenantUserOrganizationContextAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindSingleAsync(
            predicate: u => u.Id == userId && u.Organization != null,
            includeProperties: u => u.Organization!,
            cancellationToken: cancellationToken);

        if (user == null)
        {
            throw new NotFoundException($"User organization context with Id {userId} is not found");
        }

        return new UserOrganizationContext
        {
            OrganizationId = user!.Organization!.Id,
            OrganizationContextPath = user.Organization.Context.Path, // important Ring-fencing
            OrganizationName = user.Organization.Name,
            OrganizationNumber = user.Organization.OrganizationNumber,
            OrganizationPhoneNumber = user.Organization.PhoneNumber,
            OrganizationEmail = user.Organization.Email,
            OrganizationStreet = user.Organization.Address?.Street,
            OrganizationCity = user.Organization.Address?.City,
            OrganizationCountry = user.Organization.Address?.Country,
            UserId = user.Id,
            UserDisplayName = user.Name,
            UserEmail = user.Email,
            UserPhoneNumber = user.PhoneNumber,
        };
    }

    public async Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId, CancellationToken cancellationToken)
    {
        try
        {
            var operationalUser = _userDetailsProvider.AuthenticatedUser;
            _userDetailsProvider.SetSystemUserContext(operationalUser.TenantId);

            var organizationContext = await CalculateOrganizationContext(organizationId, cancellationToken);

            return organizationContext;
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task<OrganizationContext> CalculateOrganizationContext(string organizationId, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.FindByIdAsync(organizationId, cancellationToken: cancellationToken);

        if (organization == null)
        {
            throw new NotFoundException($"Organization context with Id {organizationId} is not found.");
        }

        return new OrganizationContext
        {
            OrganizationId = organization.Id,
            OrganizationName = organization.Name,
            OrganizationContextPath = organization.Context.Path,
            OrganizationNumber = organization.OrganizationNumber,
            OrganizationPhoneNumber = organization.PhoneNumber,
            OrganizationEmail = organization.Email,
            OrganizationStreet = organization.Address?.Street,
            OrganizationCity = organization.Address?.City,
            OrganizationCountry = organization.Address?.Country
        };
    }

    public async Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath, CancellationToken cancellationToken)
    {
        try
        {
            var authenticatedUser = _userDetailsProvider.AuthenticatedUser;
            _userDetailsProvider.SetSystemUserContext(authenticatedUser.TenantId);

            var organizationContext = await CalculateOrganizationContextByPath(organizationContextPath, cancellationToken);

            return organizationContext;
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task<OrganizationContext> CalculateOrganizationContextByPath(string organizationContextPath, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.FindSingleAsync(o => o.Context.Path == organizationContextPath, cancellationToken: cancellationToken);

        if (organization == null)
        {
            throw new NotFoundException($"Organization context with context path {organizationContextPath} is not found.");
        }

        return new OrganizationContext
        {
            OrganizationId = organization.Id,
            OrganizationName = organization.Name,
            OrganizationContextPath = organization.Context.Path,
            OrganizationNumber = organization.OrganizationNumber,
            OrganizationPhoneNumber = organization.PhoneNumber,
            OrganizationEmail = organization.Email,
            OrganizationStreet = organization.Address?.Street,
            OrganizationCity = organization.Address?.City,
            OrganizationCountry = organization.Address?.Country
        };
    }
}
