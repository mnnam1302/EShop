namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public interface SupportedFeaturesUpdated : TenancyEvent
{
    string SourceSystemReference { get; }
    IFeature[] Features { get; }
    SupportedFeaturesAction Action { get; }
}

public interface ITenantFeaturesUpdated : TenancyEvent
{ }

public interface IFeature
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string State { get; }
    string Module { get; }
}

public enum SupportedFeaturesAction
{
    AddOrUpdate,
    Delete
}