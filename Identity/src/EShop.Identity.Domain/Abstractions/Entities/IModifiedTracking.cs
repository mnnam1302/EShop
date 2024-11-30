namespace EShop.Identity.Domain.Abstractions.Entities;

public interface IModifiedTracking
{
    string? ModifiedBy { get; set; }
    DateTimeOffset? ModifiedDate { get; set; }
}