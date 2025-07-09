namespace EShop.Shared.Sequences;

public interface IUniqueReferenceGenerator
{
    Task<string> CreateReference(string sequenceName);
}
