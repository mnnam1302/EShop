using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public static class Command
{
    public sealed record UpdateSupportedFeaturesInternalCommand : ICommand
    {
        public required string SourceSystemReference { get; init; }

        public IFeature[] Features { get; init; } = [];

        public SupportedFeaturesAction Action { get; init; }

        public required string TenantId { get; init; }

        public required string ActionUserId { get; init; }
    }

    public sealed record UpdateTenantFeaturesCommand(string TenantId) : ICommand;
}