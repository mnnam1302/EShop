namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public class SupportedFeaturesUpdated : TenancyEvent
{
    public required string SourceSystemReference { get; init; }
    public required IFeature[] Features { get; init; }
    public required SupportedFeaturesAction Action { get; init; }
}

public interface IFeature
{
    string Id { get; init; }
    string Name { get; init; }
    string Description { get; init; }
    string State { get; init; }
    string Module { get; init; }
}

public enum SupportedFeaturesAction
{
    AddOrUpdate,
    Delete
}

public sealed class TenantFeaturesUpdated : TenancyEvent { }