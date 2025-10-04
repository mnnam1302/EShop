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
        public static readonly Error InvalidRefreshToken = new("Authentication.InvalidRefreshToken", "The provided refresh token is invalid.");
        public static readonly Error RefreshTokenExpired = new("Authentication.RefreshTokenExpired", "The provided refresh token has expired.");
    }
}