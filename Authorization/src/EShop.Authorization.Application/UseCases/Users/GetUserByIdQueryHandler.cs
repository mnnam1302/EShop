using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;

namespace EShop.Authorization.Application.UseCases.Users;

public sealed class GetUserByIdQuery(string userId) : IQuery<UserDetailsResponse>
{
    public string UserId { get; init; } = userId;
}

public sealed class UserDetailsResponse
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required string PhoneNumber { get; init; }
    public required string TenantId { get; init; }
    public required string CreatedByUserId { get; init; }
    public required string OrganizationId { get; init; }
}

internal sealed class GetUserByIdQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserByIdQuery, UserDetailsResponse>
{
    public async Task<Result<UserDetailsResponse>> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByIdAsync(query.UserId, cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDetailsResponse>(new("User.NotFound", $"User {query.UserId} is not found."));
        }

        var response = new UserDetailsResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.Name,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            OrganizationId = user.OrganizationId!,
            CreatedByUserId = user.CreatedByUserId,
        };

        return Result.Success(response);
    }
}
