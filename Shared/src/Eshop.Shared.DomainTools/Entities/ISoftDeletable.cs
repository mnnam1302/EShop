namespace Eshop.Shared.DomainTools.Entities;

public interface ISoftDeletable
{
    string DeletedBy { get; set; }
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedOnUtc { get; set; }
}