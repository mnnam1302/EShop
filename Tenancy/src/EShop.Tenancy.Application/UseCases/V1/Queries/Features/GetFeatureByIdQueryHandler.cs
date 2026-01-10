using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Tenancy.Domain.Repositories;

namespace EShop.Tenancy.Application.UseCases.V1.Queries.Features
{
    public sealed class GetFeatureByIdQuery(string featureId) : IQuery<FeatureResponse>
    {
        public string FeatureId { get; } = featureId;
    }

    public sealed class FeatureResponse
    {
        public required string FeatureId { get; init; }
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
            var feature = await _featureRepository.FindByIdAsync(query.FeatureId, cancellationToken: cancellationToken);

            if (feature is null)
            {
                throw new NotFoundException($"Feature with id '{query.FeatureId}' was not found.");
            }

            var response = new FeatureResponse
            {
                FeatureId = feature.Id,
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