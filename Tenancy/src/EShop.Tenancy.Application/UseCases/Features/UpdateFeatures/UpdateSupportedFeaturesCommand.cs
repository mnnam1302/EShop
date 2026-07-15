using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Services.Tenancy.Features;

namespace EShop.Tenancy.Application.UseCases.Features.UpdateFeatures;

public sealed class UpdateSupportedFeaturesCommand : ICommand
{
    public required string SourceSystemReference { get; init; }

    public IFeature[] Features { get; init; } = [];

    public SupportedFeaturesAction Action { get; init; }

    public required string TenantId { get; init; }

    public required string ActionUserId { get; init; }
}
