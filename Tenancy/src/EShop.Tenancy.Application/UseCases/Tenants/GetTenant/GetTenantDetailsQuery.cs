using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Tenancy.Application.UseCases.Tenants.GetTenant;

public sealed record GetTenantDetailsQuery(string TenantId) : IQuery<TenantDetailsResponse>;
