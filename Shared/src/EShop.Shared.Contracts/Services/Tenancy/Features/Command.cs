using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public static class Command
{
    public record UpdateSupportedFeaturesCommand : ICommand
    {
        public string SourceSystemReference { get; init; }

        public IFeature[] Features { get; init; }

        public SupportedFeaturesAction Action { get; init; }

        public string TenantId { get; init; }

        public string ActionUserId { get; init; }
    }
}