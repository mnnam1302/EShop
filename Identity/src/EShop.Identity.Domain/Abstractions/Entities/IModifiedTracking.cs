namespace EShop.Identity.Domain.Abstractions.Entities;

public interface IModifiedTracking
{
    DateTimeOffset? ModifiedDate { get; set; }
    string? ModifiedBy { get; set; }
}