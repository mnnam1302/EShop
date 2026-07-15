using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Tenancy.Application.UseCases.Features.GetFeature;

public sealed class GetFeatureByIdQuery(string id) : IQuery<FeatureResponse>
{
    public string Id { get; } = id;
}
