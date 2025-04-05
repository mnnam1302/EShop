using EShop.Shared.Contracts.Abstractions.Requests;
using Newtonsoft.Json;

namespace EShop.Shared.Contracts.Services.Identity.Organizations;

public static class Command
{
    public sealed record CreateOrganizationCommand(
        string Name, 
        string Email,
        string? OrganizationNumber,
        string? PhoneNumber,
        string? Address,
        string? City,
        string? PostCode,
        string? Description,
        string ParentOrganizationId) : ICommand;

    public sealed record UpdateOrganizationCommand : ICommand
    {
        [JsonIgnore]
        public string? Id { get; init; }
        public string Name { get; init; }
        public string? OrganizationNumber { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? PostCode { get; init; }
        public string? Description { get; init; }
        public string? ParentOrganizationId { get; init; }
    }

    public record DeleteOrganization(string Id) : ICommand;

    public record AddUserToOrganization(string OrganizationId, string UserId) : ICommand;

    public record RemoveUserFromOrganization(string OrganizationId, string UserId) : ICommand;

    public record AddRoleToOrganization(string OrganizationId, string RoleId) : ICommand;

    public record RemoveRoleFromOrganization(string OrganizationId, string RoleId) : ICommand;

    public record AddPermissionToOrganization(string OrganizationId, string PermissionId) : ICommand;

    public record RemovePermissionFromOrganization(string OrganizationId, string PermissionId) : ICommand;

    public record AddChildOrganization(string ParentId, string ChildId) : ICommand;

    public record RemoveChildOrganization(string ParentId, string ChildId) : ICommand;
}