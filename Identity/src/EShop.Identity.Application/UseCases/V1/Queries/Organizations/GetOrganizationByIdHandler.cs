using AutoMapper;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.DomainTools.DomainExceptions;

namespace EShop.Identity.Application.UseCases.V1.Queries.Organizations;

public class GetOrganizationByIdHandler : IQueryHandler<Query.GetOrganizationById, Response.OrganizationResponse>
{
    private readonly IIdentityAggregateRepository<Organization, string> _organizationRepository;
    private readonly IMapper _mapper;

    public GetOrganizationByIdHandler(
        IIdentityAggregateRepository<Organization, string> organizationRepository,
        IMapper mapper)
    {
        _organizationRepository = organizationRepository;
        _mapper = mapper;
    }

    public async Task<Result<Response.OrganizationResponse>> Handle(Query.GetOrganizationById request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.FindByIdAsync(request.Id);
        if (organization == null)
        {
            throw new NotFoundException("Organization is not found");
        }

        var result = _mapper.Map<Response.OrganizationResponse>(organization);
        return Result.Success(result);
    }
}