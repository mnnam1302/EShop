namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public class SupportedFeaturesUpdated : TenancyEvent
{
    public required string SourceSystemReference { get; init; }
    public required IFeature[] Features { get; init; }
    public required SupportedFeaturesAction Action { get; init; }
}

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

public sealed class TenantFeaturesUpdated : TenancyEvent { }
