using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Inventory.Infrastructure.Producers;

internal sealed class InventoryFeatureRegistrationService : IFeatureRegistrationService
{
    private readonly IEventBus _eventBus;

    public InventoryFeatureRegistrationService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    private static readonly InventoryFeature[] _features =
    [
        new()
         {
             Id = FeatureConstants.Inventory.InventoryManagement,
             Name = "Inventory Management",
             Description = "Inventory Management"
         }
    ];

    public async Task RegisterFeatures()
    {
        await _eventBus.PublishAsync(new SupportedFeaturesUpdated
        {
            SourceSystemReference = "Inventory",
            Features = _features,
            Action = SupportedFeaturesAction.AddOrUpdate,
            TenantId = UserData.SystemTenantId,
            ActionUserId = UserData.SystemUsername,
            ActionUserType = UserTypes.SystemUsers
        });
    }

    private sealed class InventoryFeature : IFeature
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public string State { get; init; } = nameof(FeatureState.Enabled);
        public string Module { get; init; } = nameof(FeatureModules.EShop_Inventory);
    }
}
