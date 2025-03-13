using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;

namespace EShop.Identity.Application.Services;

public interface IUserOrganizationContextCalculator
{
    Task<UserOrganizationContext> GetUserOrganizationContextAsync();

    Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = UserTypes.TenantUsers);

    Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId);

    Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath);
}

public class UserOrganizationContextCalculator : IUserOrganizationContextCalculator, IUserOrganizationContextProvider
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IIdentityRepositoryBase<User, string> _userRepository;

    public UserOrganizationContextCalculator(
        IUserDetailsProvider userDetailsProvider, IIdentityRepositoryBase<User, string> userRepository)
    {
        _userDetailsProvider = userDetailsProvider;
        _userRepository = userRepository;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync()
    {
        var authenticatedUser = _userDetailsProvider.AuthenticatedUser;

        var userOrganizationContext = await CalculateUserOrganizationContextInternal(
            authenticatedUser.Id,
            authenticatedUser.UserType,
            authenticatedUser.TenantId)
            ?? throw new UserOrganizationContextNotFound("User organization is not found");

        return userOrganizationContext;
    }

    private async Task<UserOrganizationContext> CalculateUserOrganizationContextInternal(string userId, string userType, string tenantId)
    {
        return userType switch
        {
            UserTypes.SystemUsers => await GetTenantUserOrganizationContextAsync(userId),
            _ => await GetTenantUserOrganizationContextAsync(userId)
        };
    }

    private async Task<UserOrganizationContext> GetTenantUserOrganizationContextAsync(string userId)
    {
        var userOrganizationContext = await _userRepository
            .FindByCondition(
                predicate: u => u.Id == userId && u.Organization != null,
                trackChanges: false,
                includeProperties: u => u.Organization!)
            .Select(u => new UserOrganizationContext
            {
                OrganizationId = u.Organization!.Id,
                OrganizationContextPath = u.Organization.OrganizationNumber, // important
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
            .SingleOrDefaultAsync() ?? throw new UserOrganizationContextNotFound("User organization is not found");

        return userOrganizationContext;
    }

    public Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = UserTypes.TenantUsers)
    {
        throw new NotImplementedException();
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