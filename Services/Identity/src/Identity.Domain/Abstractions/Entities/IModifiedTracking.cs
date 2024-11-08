namespace Identity.Domain.Abstractions.Entities;

public interface IModifiedTracking
{
    DateTimeOffset? ModifiedDate { get; set; }
    string? ModifiedBy { get; set; }
}