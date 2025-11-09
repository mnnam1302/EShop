namespace EShop.Authorization.API.Models;

public sealed class CreateChildOrganizationRequest
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? OrganizationNumber { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Description { get; init; }

    public string? Street { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? Country { get; init; }
    public string? ZipCode { get; init; }
}
