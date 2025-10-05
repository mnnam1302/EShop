# Authorization Service

## Overview

This service provides RSA-based JWT token generation and validation for multi-tenant scenarios. Each tenant has a unique RSA key pair, supporting secure, isolated authentication.

## Add new migration
```bash
dotnet ef migrations add <MigrationName> -p ../EShop.Authorization.Infrastructure --startup-project .
```


## Key Features

1. **RSA Asymmetric Encryption**: 2048-bit RSA keys for JWT signing
2. **Tenant-Specific Keys**: Each tenant has a unique key pair
3. **Automatic Key Rotation**: Keys expire and rotate every 30 days
4. **Public Key Distribution**: Endpoints for public key retrieval and JWKS
5. **JWT/OAuth2 Compliance**: Follows standards for interoperability

## Architecture

```
Login Request -> LoginQueryHandler -> RSA Key Pair Generation
                                   -> JWT Token with RSA Signature
                                   -> Public Key APIs for Distribution
```

## How It Works

### 1. Login Process

```csharp
// 1. User authenticates
var loginQuery = new LoginQuery("username", "password");
var result = await mediator.QueryAsync<LoginQuery, AuthenticationResponse>(loginQuery);

// 2. System ensures RSA key pair exists for tenant
await rsaKeyManager.GetActiveKeyPairAsync(tenantId) ?? 
    await rsaKeyManager.GenerateKeyPairAsync(tenantId);

// 3. JWT token is signed with tenant's private key
var accessToken = await jwtTokenManager.GenerateAccessTokenAsync(claims, tenantId);
```

### 2. Token Structure

JWT tokens include:
- **Header**: Algorithm (RS256), Key ID
- **Payload**: Standard and tenant-specific claims
- **Signature**: RSA-SHA256 using tenant's private key

Example payload:
```json
{
  "sub": "user123",
  "name": "John Doe",
  "tenant_id": "tenant456",
  "user_type": "user",
  "jti": "unique-token-id",
  "iat": 1640995200,
  "key_id": "rsa-key-guid",
  "exp": 1641081600,
  "iss": "EShop.Authorization",
  "aud": "EShop.Services"
}
```

## Public Key Distribution

Endpoints:
- **Get Active Public Key:** `GET /api/v1/auth/public-key/{tenantId}`
- **Get Specific Public Key:** `GET /api/v1/auth/public-key/{tenantId}/{keyId}`
- **JWKS (OAuth2):** `GET /api/v1/auth/.well-known/jwks/{tenantId}`

### JWKS Response Example
```json
{
  "keys": [
    {
      "kty": "RSA",
      "use": "sig",
      "kid": "rsa-key-guid",
      "alg": "RS256",
      "n": "...",
      "e": "..."
    }
  ]
}
```

## Integration with Other Microservices

### 1. Configure JWT Authentication

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "EShop.Authorization",
            ValidAudience = "EShop.Services",
            IssuerSigningKeyResolver = async (token, securityToken, kid, parameters) =>
            {
                // Fetch public key from Authorization service
                var publicKey = await GetPublicKeyFromAuthService(tenantId, kid);
                return new[] { new RsaSecurityKey(publicKey) { KeyId = kid } };
            }
        };
    });
```

### 2. Public Key Fetching Example

```csharp
public class PublicKeyService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public async Task<RSA> GetPublicKeyAsync(string tenantId, string keyId)
    {
        var cacheKey = $"publickey_{tenantId}_{keyId}";
        if (_cache.TryGetValue(cacheKey, out RSA cachedKey))
            return cachedKey;

        var response = await _httpClient.GetAsync($"/api/v1/auth/public-key/{tenantId}/{keyId}");
        var publicKeyData = await response.Content.ReadFromJsonAsync<PublicKeyResponse>();
        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyData.PublicKeyPem);
        _cache.Set(cacheKey, rsa, publicKeyData.ExpiresAt);
        return rsa;
    }
}
```

## Security Considerations

- **Key Rotation:** Automatic every 30 days
- **Tenant Isolation:** Separate key pairs per tenant
- **Secure Storage:** Private keys in distributed cache
- **Algorithm:** RS256 (RSA-SHA256)

## Monitoring and Logging

- Key pair generation and rotation
- Token generation (no sensitive data)
- Public key distribution requests

## Configuration

`appsettings.json`:
```json
{
  "JwtOptions": {
    "Issuer": "EShop.Authorization",
    "Audience": "EShop.Services",
    "AccessTokenExpiryHours": 1,
    "RefreshTokenExpiryHours": 168
  }
}
```

## Error Handling

- `401 Unauthorized`: Invalid credentials on login
- `404 Not Found`: Public key not found for tenant/key
- `500 Internal Server Error`: Unexpected errors

## Extensibility & Multi-Tenancy

- Add new tenants by provisioning a key pair
- JWKS endpoint supports OAuth2/OpenID Connect federation
- Key rotation and caching are fully automated

## Benefits

- **Scalability:** Services validate tokens independently
- **Security:** Private keys never leave the Authorization service
- **Multi-tenancy:** Isolated key pairs
- **Standards Compliance:** JWT/OAuth2 best practices
- **Key Management:** Automatic rotation and distribution

---

For more details, see the implementation in `AuthenticationEndpointHandler`, `LoginQueryHandler`, `RsaKeyManager`, and `JwtTokenManager`.