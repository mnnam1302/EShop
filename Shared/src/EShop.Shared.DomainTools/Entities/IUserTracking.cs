namespace EShop.Shared.DomainTools.Entities;

public interface IUserTracking
{
    string CreatedByUserId { get; set; }
    string? LastModifiedByUserId { get; set; }
}