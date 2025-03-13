using EShop.Shared.Contracts.Services.Tenancy.Features;
using MediatR;

namespace EShop.Tenancy.Infrastructure.Consumers;

public static class FeatureConsumer
{
    public class SupportedFeaturesUpdatedConsumer : Consumer<SupportedFeaturesUpdated>
    {
        public SupportedFeaturesUpdatedConsumer(ISender sender) : base(sender)
        {
        }
    }
}