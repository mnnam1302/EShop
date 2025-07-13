namespace EShop.Shared.DomainTools.Entities;

public interface IDateTracking
{
    DateTimeOffset CreatedAtUtc { get; set; }
    DateTimeOffset? LastModifiedAtUtc { get; set; }
}