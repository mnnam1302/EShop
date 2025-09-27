using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Authorization.Domain.Constants;

public static class ErrorCodes
{
    public static class Organization
    {
        public static readonly Error AlreadyExists = new("Organization.AlreadyExists", "An organization with this tenant ID already exists.");
        public const string CreationFailed = "Organization.CreationFailed";
    }

    public static class Role
    {
        public const string CreationFailed = "Role.CreationFailed";
        public const string PermissionAssignmentFailed = "Role.PermissionAssignmentFailed";
    }

    public static class User
    {
        public const string CreationFailed = "User.CreationFailed";
        public const string RoleAssignmentFailed = "User.RoleAssignmentFailed";
    }

    public static class General
    {
        public const string UnexpectedError = "General.UnexpectedError";
        public const string ValidationFailed = "General.ValidationFailed";
    }
}