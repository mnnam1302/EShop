using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public interface SupportedFeaturesUpdated : TenancyEvent
{
    public string SourceSystemReference { get; }
    public IFeature[] Features { get; }
    public SupportedFeaturesAction Action { get; }
}

public interface TenantFeaturesUpdated : TenancyEvent { }

public interface IFeature
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Module { get; }
    string State { get; }
}

public enum SupportedFeaturesAction
{
    AddOrUpdate,
    Delete
}