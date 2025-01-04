namespace Eshop.Shared.DomainTools.Entities;

public interface IDateTracking
{
    DateTimeOffset CreatedOnUtc { get; set; }
    DateTimeOffset? LastModifiedOnUtc { get; set; }
}