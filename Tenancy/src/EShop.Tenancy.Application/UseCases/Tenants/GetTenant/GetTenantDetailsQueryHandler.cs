using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using EShop.Tenancy.Domain.Abstractions.Repositories;

namespace EShop.Tenancy.Application.UseCases.Tenants.GetTenant;

internal sealed class GetTenantDetailsQueryHandler : IQueryHandler<GetTenantDetailsQuery, TenantDetailsResponse>
{
    private readonly ITenantRepository tenantRepository;

    public GetTenantDetailsQueryHandler(ITenantRepository tenantRepository)
    {
        this.tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantDetailsResponse>> HandleAsync(GetTenantDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var tenant = await tenantRepository.FindByIdAsync(query.TenantId, cancellationToken: cancellationToken);

        if (tenant is null)
        {
            return Result.Failure<TenantDetailsResponse>(new("Tenant.NotFound", $"Tenant with ID '{query.TenantId}' not found."));
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
