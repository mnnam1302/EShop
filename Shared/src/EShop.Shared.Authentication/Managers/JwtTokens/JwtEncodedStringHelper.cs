namespace EShop.Shared.Authentication.Managers.JwtTokens;

public static class JwtEncodedStringHelper
{
    /// <summary>
    /// Gets the base64-encoded string from the JWT Bearer token.
    /// </summary>
    /// <param name="accessToken">The JWT Bearer token as a base64-encoded string.</param>
    /// <returns>The JWT token as a base64-encoded string without the Bearer scheme.</returns>
    public static string GetJwtEncodedString(string accessToken)
    {
        // CRM services may send the token with the duplicated "Bearer " scheme,
        // e.g. "Bearer Bearer <token>", so we need to remove all occurrences of "Bearer ".
        return accessToken.Replace("Bearer ", string.Empty, StringComparison.InvariantCultureIgnoreCase);
    }
}