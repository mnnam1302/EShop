using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Tenancy.Domain.Repositories;

namespace EShop.Tenancy.Application.UseCases.V1.Queries.Tenants;

public sealed record GetTenantDetailsQuery(string TenantId) : IQuery<TenantDetailsResponse>;

public sealed class TenantDetailsResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string OwnerUsername { get; init; } = string.Empty;
    public string OwnerEmail { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Description { get; init; }
}

internal sealed class GetTenantDetailsQueryHandler : IQueryHandler<GetTenantDetailsQuery, TenantDetailsResponse>
{
    private readonly ITenantRepository tenantRepository;

    public GetTenantDetailsQueryHandler(ITenantRepository tenantRepository)
    {
        this.tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantDetailsResponse>> Handle(GetTenantDetailsQuery request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.FindByIdAsync(request.TenantId, cancellationToken: cancellationToken);

        if (tenant is null)
        {
            return Result.Failure<TenantDetailsResponse>(new("Tenant.NotFound", $"Tenant with ID '{request.TenantId}' not found."));
        }

        var response = new TenantDetailsResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            OwnerUsername = tenant.OwnerUsername ?? string.Empty,
            OwnerEmail = tenant.Email ?? string.Empty,
            PhoneNumber = tenant.PhoneNumber,
            Description = tenant.Description
        };

        return Result.Success(response);
    }
}
