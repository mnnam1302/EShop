namespace EShop.Identity.Domain.Abstractions.Entities;

public interface ICreatedTracking
{
    DateTimeOffset CreatedDate { get; set; }
    string? CreatedBy { get; set; }
}