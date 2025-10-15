using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Identity.Application.Services;

public interface IUserOrganizationContextCalculator
{
    Task<UserOrganizationContext> GetUserOrganizationContextAsync();

    Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = UserTypes.TenantUsers);

    Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId);

    Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath);
}

public class UserOrganizationContextCalculator : IUserOrganizationContextCalculator
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ILogger<UserOrganizationContextCalculator> _logger;

    public UserOrganizationContextCalculator(
        IUserDetailsProvider userDetailsProvider,
        IIdentityRepositoryBase<User, string> userRepository,
        IOrganizationRepository organizationRepository,
        ILogger<UserOrganizationContextCalculator> logger)
    {
        _userDetailsProvider = userDetailsProvider;
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _logger = logger;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync()
    {
        try
        {
            var authenticatedUser = _userDetailsProvider.AuthenticatedUser;
            _userDetailsProvider.SetSystemUserContext(authenticatedUser.TenantId);

            var userOrganizationContext = await CalculateUserOrganizationContextInternal(
                authenticatedUser.Id,
                authenticatedUser.UserType,
                authenticatedUser.TenantId)
                ?? throw new NotFoundException("User organization is not found");

            return userOrganizationContext;
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task<UserOrganizationContext?> CalculateUserOrganizationContextInternal(string userId, string userType, string tenantId)
    {
        return userType switch
        {
            UserTypes.SystemUsers => await GetTenantUserOrganizationContextAsync(userId),
            _ => await GetTenantUserOrganizationContextAsync(userId)
        };
    }

    private async Task<UserOrganizationContext?> GetTenantUserOrganizationContextAsync(string userId)
    {
        var userOrganization = await _userRepository
            .FindByCondition(
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
                OrganizationAddress = u.Organization.Address,
                OrganizationCity = u.Organization.City,
                OrganizationPostcode = u.Organization.Postcode,
                UserId = u.Id,
                UserDisplayName = u.DisplayName,
                UserEmail = u.Email,
                UserPhoneNumber = u.PhoneNumber,
            })
            .SingleOrDefaultAsync();

        if (userOrganization == null)
        {
            _logger.LogInformation("The tenant user with id '{userId}' was not found", userId);
            return null;
        }

        _logger.LogDebug(
                "The tenant user organization context path '{orgContextPath}' was retrieved for userId '{id}', userType '{userType}'",
                userOrganization.OrganizationContextPath,
                userId,
                UserTypes.TenantUsers);

        return userOrganization;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = UserTypes.TenantUsers)
    {
        try
        {
            var authenticatedUser = _userDetailsProvider.AuthenticatedUser;
            _userDetailsProvider.SetSystemUserContext(authenticatedUser.TenantId);

            var userOrganizationContext = await CalculateUserOrganizationContextInternal(
                userId,
                typeUser,
                authenticatedUser.TenantId)
                ?? throw new NotFoundException($"User organization context with Id {userId} is not found");

            return userOrganizationContext;
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    public async Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId)
    {
        try
        {
            var authenticatedUser = _userDetailsProvider.AuthenticatedUser;
            _userDetailsProvider.SetSystemUserContext(authenticatedUser.TenantId);

            var organizationContext = await CalculateOrganizationContextInternal(organizationId)
                ?? throw new NotFoundException($"Organization context with ID {organizationId} is not found");

            return organizationContext;
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task<OrganizationContext?> CalculateOrganizationContextInternal(string organizationId)
    {
        var organizationContext = await _organizationRepository
            .FindByCondition(o => o.Id == organizationId)
            .Select(o => new OrganizationContext
            {
                OrganizationId = o.Id,
                OrganizationName = o.Name,
                OrganizationContextPath = o.Context.Path,
                OrganizationNumber = o.OrganizationNumber,
                OrganizationPhoneNumber = o.PhoneNumber,
                OrganizationEmail = o.Email,
                OrganizationAddress = o.Address,
                OrganizationCity = o.City,
                OrganizationPostcode = o.Postcode
            })
            .SingleOrDefaultAsync();

        _logger.LogDebug(
            "The context '{context}' was retrieved for organization '{organizationId}'",
            organizationContext?.OrganizationContextPath,
            organizationId);

        return organizationContext;
    }

    public async Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath)
    {
        try
        {
            var authenticatedUser = _userDetailsProvider.AuthenticatedUser;
            _userDetailsProvider.SetSystemUserContext(authenticatedUser.TenantId);

            var organizationContext = await CalculateOrganizationContextByPathInternal(organizationContextPath)
                ?? throw new NotFoundException($"Organization context with context path {organizationContextPath} is not found");

            return organizationContext;
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task<OrganizationContext?> CalculateOrganizationContextByPathInternal(string organizationContextPath)
    {
        var organizationContext = await _organizationRepository
            .FindByCondition(o => o.Context.Path == organizationContextPath)
            .Select(o => new OrganizationContext
            {
                OrganizationId = o.Id,
                OrganizationName = o.Name,
                OrganizationContextPath = o.Context.Path,
                OrganizationNumber = o.OrganizationNumber,
                OrganizationPhoneNumber = o.PhoneNumber,
                OrganizationEmail = o.Email,
                OrganizationAddress = o.Address,
                OrganizationCity = o.City,
                OrganizationPostcode = o.Postcode
            })
            .SingleOrDefaultAsync();

        if (organizationContext == null)
        {
            _logger.LogDebug("The organization with path '{OrganizationContextPath}' was not found", organizationContextPath);
        }
        else
        {
            _logger.LogDebug(
                "The context '{OrganizationContextPath}' was retrieved for organization '{OrganizationId}'",
                organizationContextPath,
                organizationContext.OrganizationId);
        }

        return organizationContext;
    }
}