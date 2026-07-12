using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Tenancy.Domain.Abstractions.Repositories;

namespace EShop.Tenancy.Application.UseCases.V1.Queries.Features
{
    public sealed class GetFeatureByIdQuery(string id) : IQuery<FeatureResponse>
    {
        public string Id { get; } = id;
    }

    public sealed class FeatureResponse
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required string State { get; init; }
        public required string Module { get; init; }
        public required string DefaultStateForNewTenant { get; init; }
        public string? Category { get; init; }
    }

    internal sealed class GetFeatureByIdQueryHandler : IQueryHandler<GetFeatureByIdQuery, FeatureResponse>
    {
        private readonly IFeatureRepository _featureRepository;

        public GetFeatureByIdQueryHandler(IFeatureRepository featureRepository)
        {
            _featureRepository = featureRepository;
        }

        public async Task<Result<FeatureResponse>> HandleAsync(GetFeatureByIdQuery query, CancellationToken cancellationToken = default)
        {
            var feature = await _featureRepository.FindByIdAsync(query.Id, cancellationToken: cancellationToken);

            if (feature is null)
            {
                throw new NotFoundException($"Feature with id '{query.Id}' was not found.");
            }

            var response = new FeatureResponse
            {
                Id = feature.Id,
                Name = feature.Name,
                Description = feature.Description,
                State = feature.State,
                Module = feature.Module,
                DefaultStateForNewTenant = feature.DefaultStateForNewTenant,
                Category = feature.Category
            };

            return Result.Success(response);
        }
    }
}