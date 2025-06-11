using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Identity.Application.UseCases.V1.Queries.Users;

public sealed class GetUserQueryHandler : IQueryHandler<Query.GetUserQuery, Response.UserDetailsResponse>
{
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly IPermissionValidator _permissionValidator;
    private readonly IFeatureValidator _featureValidator;

    public GetUserQueryHandler(
        IIdentityRepositoryBase<User, string> userRepository,
        IPermissionValidator permissionValidator,
        IFeatureValidator featureValidator)
    {
        _userRepository = userRepository;
        _permissionValidator = permissionValidator;
        _featureValidator = featureValidator;
    }

    public async Task<Result<Response.UserDetailsResponse>> Handle(Query.GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(request.UserId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"User {request.UserId} is not found.");

        if (user is IAccessControlled accessControlled)
        {
            await accessControlled.PopulateActions(_permissionValidator, user, _featureValidator);
        }

        var response = MapFromUser(user);
        return Result.Success(response);
    }

    private static Response.UserDetailsResponse MapFromUser(User source)
    {
        return new Response.UserDetailsResponse
        {
            Id = source.Id,
            Username = source.Username,
            DisplayName = source.DisplayName,
            Email = source.Email,
            PhoneNumber = source.PhoneNumber,
            DateOfBirth = source.DateOfBirth,
            OrganizationId = source.OrganizationId,
            Actions = source.Actions.ToDictionary(
                kvp => kvp.Key,
                kvp => new Shared.Contracts.Shared.ActionDefinition(kvp.Value.IsAllowed)
            )
        };
    }
}