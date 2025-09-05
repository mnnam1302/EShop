using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using Microsoft.EntityFrameworkCore;

namespace EShop.Configuration.Application.Agencies.GetAgencies;

public sealed class GetAgenciesQueryHandler : IQueryHandler<GetAgenciesQuery, List<GetAgenciesResponse>>
{
    private readonly IAgencyRepository _agencyRepository;

    public GetAgenciesQueryHandler(IAgencyRepository agencyRepository)
    {
        _agencyRepository = agencyRepository;
    }

    public async Task<Result<List<GetAgenciesResponse>>> HandleAsync(GetAgenciesQuery query, CancellationToken cancellationToken = default)
    {
        var agencies = await _agencyRepository
            .FindAll()
            .ToListAsync(cancellationToken);

        return agencies
            .Select(a => new GetAgenciesResponse
            {
                Id = a.Id,
                Name = a.Name,
                TenantId = a.TenantId
            })
            .ToList();
    }
}
