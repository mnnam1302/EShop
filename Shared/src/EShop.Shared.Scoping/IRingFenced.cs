namespace EShop.Shared.Scoping;

public interface IRingFenced
{
    string Scope { get; }
}

public interface IAllowWildcardRingFencing
{
    bool AllowChildScopeAccess { get; }
}