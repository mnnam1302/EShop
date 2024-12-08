namespace EShop.Identity.Domain.Abstractions.Entities;

public interface ICreatedTracking
{
    string? CreatedBy { get; set; }
    DateTimeOffset CreatedDate { get; set; }
}