namespace Eshop.Shared.DomainTools.Entities;

public interface IUserTracking
{
    string CreatedBy { get; set; }
    string? LastModifiedBy { get; set; }
}