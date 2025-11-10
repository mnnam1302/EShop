using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Authorization.Domain.Constants;

public static class ErrorContants
{
    public static class Organization
    {
        public static readonly Error AlreadyExists = new("Organization.AlreadyExists", "An organization with this tenant ID already exists.");
        public static readonly Error NotFound = new("Organization.NotFound", "The specified organization does not exist.");
    }

    public static class Authentication
    {
        public static readonly Error UserNotFound = new("Authentication.UserNotFound", "The user is not found.");
        public static readonly Error InvalidCredentials = new("Authentication.InvalidCredentials", "The provided credentials are invalid.");
        public static readonly Error InvalidPassword = new("Authentication.InvalidPassword", "The provided password is incorrect.");
        public static readonly Error InvalidToken = new("Authentication.InvalidToken", "The provided token is invalid or malformed");
        public static readonly Error TokenInvalidCache = new("Authentication.TokenInvalidCache", "The token is not found in cache or has been revoked");
    }

    public static class User
    {
        public static readonly Error PermissionNotFound = new("User.PermissionNotFound", "The specified permission is not found.");
    }
}