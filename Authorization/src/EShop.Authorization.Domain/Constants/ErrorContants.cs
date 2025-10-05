using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Authorization.Domain.Constants;

public static class ErrorContants
{
    public static class Organization
    {
        public static readonly Error AlreadyExists = new("Organization.AlreadyExists", "An organization with this tenant ID already exists.");
    }

    public static class Authentication
    {
        public static readonly Error UserNotFound = new("Authentication.UserNotFound", "The user is not found.");
        public static readonly Error InvalidCredentials = new("Authentication.InvalidCredentials", "The provided credentials are invalid.");
        public static readonly Error InvalidPassword = new("Authentication.InvalidPassword", "The provided password is incorrect.");

    }
}