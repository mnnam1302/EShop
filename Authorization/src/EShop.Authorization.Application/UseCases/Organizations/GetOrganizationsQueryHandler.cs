using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using Microsoft.EntityFrameworkCore;

namespace EShop.Authorization.Application.UseCases.Organizations;

public sealed class GetOrganizationsQuery() : IQuery<List<OrganizationsResponse>>;

public sealed class OrganizationsResponse
{
    public required string Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ParentOrganizationId { get; init; }
    public string? OrganizationNumber { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string TenantId { get; init; } = string.Empty;
}

internal sealed class GetOrganizationsQueryHandler : IQueryHandler<GetOrganizationsQuery, List<OrganizationsResponse>>
{
    private readonly IOrganizationRepository organizationRepository;

    public GetOrganizationsQueryHandler(IOrganizationRepository organizationRepository)
    {
        this.organizationRepository = organizationRepository;
    }

    public async Task<Result<List<OrganizationsResponse>>> HandleAsync(GetOrganizationsQuery query, CancellationToken cancellationToken = default)
    {
        var organizations = await organizationRepository
            .FindAll()
            .ToListAsync(cancellationToken);

        var response = organizations
            .Select(MapToResponse)
            .ToList();

        return Result.Success(response);
    }

    private OrganizationsResponse MapToResponse(Organization organization)
    {
        return new OrganizationsResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            ParentOrganizationId = organization.ParentOrganizationId,
            OrganizationNumber = organization.OrganizationNumber,
            Email = organization.Email,
            PhoneNumber = organization.PhoneNumber,
            TenantId = organization.TenantId
        };
    }
}
