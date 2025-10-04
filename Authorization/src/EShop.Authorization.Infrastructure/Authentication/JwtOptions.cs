namespace EShop.Authorization.Infrastructure.Authentication;

/// <summary>
/// JWT configuration options supporting RSA asymmetric algorithms for enhanced security
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "JwtOptions";

    /// <summary>
    /// JWT issuer (typically your authorization service URL)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// JWT audience (typically your API services)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration time in minutes (recommended: 15-60 minutes)
    /// </summary>
    public int AccessTokenExpiryMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token expiration time in days (recommended: 7-30 days)
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;

    /// <summary>
    /// Signing algorithm (RS256 recommended for RSA)
    /// </summary>
    public string Algorithm { get; set; } = "RS256";

    /// <summary>
    /// RSA key size for key generation (2048, 3072, 4096)
    /// </summary>
    public int RsaKeySize { get; set; } = 2048;

    /// <summary>
    /// Private key for signing (PEM format or key store reference)
    /// </summary>
    public string? PrivateKey { get; set; }

    /// <summary>
    /// Public key for verification (PEM format)
    /// </summary>
    public string? PublicKey { get; set; }

    /// <summary>
    /// Key identifier for rotation support
    /// </summary>
    public string KeyId { get; set; } = "default";

    /// <summary>
    /// Key rotation interval in days
    /// </summary>
    public int KeyRotationDays { get; set; } = 30;

    /// <summary>
    /// Clock skew tolerance in minutes
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;

    /// <summary>
    /// Validates the JWT options
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new ArgumentException("JWT Issuer is required");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new ArgumentException("JWT Audience is required");

        if (AccessTokenExpiryMinutes <= 0)
            throw new ArgumentException("AccessTokenExpiryMinutes must be greater than 0");

        if (RefreshTokenExpiryDays <= 0)
            throw new ArgumentException("RefreshTokenExpiryDays must be greater than 0");

        if (!IsValidAlgorithm(Algorithm))
            throw new ArgumentException($"Unsupported algorithm: {Algorithm}");

        if (RsaKeySize < 2048)
            throw new ArgumentException("RSA key size must be at least 2048 bits");
    }

    private static bool IsValidAlgorithm(string algorithm)
    {
        return algorithm switch
        {
            "RS256" or "RS384" or "RS512" or 
            "PS256" or "PS384" or "PS512" => true,
            _ => false
        };
    }
}
