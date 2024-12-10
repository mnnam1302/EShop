using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Organizations;

public static class Command
{
    public record CreateOrganization(string Name, string Description, string? ParentId, string? TenantId) : ICommand;

    public record UpdateOrganization(string Id, string Name, string Description, string? ParentId, string? TenantId) : ICommand;

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