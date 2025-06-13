namespace EShop.Shared.Scoping;

/// <summary>
/// Describes owner and place in their organisation for an entity.
/// Tenant Id is used to isolate data between tenants sharing an environment
/// Scope is used to ring-fence data within the same tenant but between different parts of their organisation.
/// </summary>
public interface IScoped
{
    public string TenantId { get; }

    public string Scope { get; }
}

/// <summary>
/// Describes entities shared across tenants.
/// </summary>
public interface IExcludedFromScoping
{
}