namespace EShop.Shared.DomainTools.Entities;

public interface IRingFenced
{
    string Scope { get; }
}

public interface IAllowWildcardRingFencing
{
    bool AllowChildScopeAccess { get; }
}