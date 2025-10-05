# RSA-Based JWT Authentication Implementation

## Overview

This implementation provides RSA asymmetric key-based JWT token generation and validation for the EShop Authorization service. Each tenant has its own RSA key pair, ensuring proper multi-tenant isolation.

## Key Features

1. **RSA Asymmetric Encryption**: Uses 2048-bit RSA keys for JWT signing
2. **Tenant-Specific Keys**: Each tenant gets its own key pair
3. **Automatic Key Rotation**: Keys expire after 30 days with automatic rotation
4. **Public Key Distribution**: Multiple endpoints for other services to fetch public keys
5. **JWT Standards Compliance**: Follows OAuth2/OpenID Connect standards

## Architecture

```
???????????????????    ????????????????????    ???????????????????
?   Login Request ?????? LoginQueryHandler ??????  RSA Key Pair   ?
???????????????????    ????????????????????    ?   Generation    ?
                                                ???????????????????
                                                         ?
                                                         ?
                                                ???????????????????
                                                ? JWT Token with  ?
                                                ?  RSA Signature  ?
                                                ???????????????????
                                                         ?
                                                         ?
                                                ???????????????????
                                                ? Public Key APIs ?
                                                ? for Distribution?
                                                ???????????????????
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

The generated JWT token includes:
- **Header**: Algorithm (RS256), Key ID
- **Payload**: Standard claims + tenant-specific claims
- **Signature**: RSA-SHA256 signature using tenant's private key

Example JWT payload:
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

Other microservices can fetch public keys through three endpoints:

### 1. Get Active Public Key
```http
GET /api/v1/auth/public-key/{tenantId}
```

### 2. Get Specific Public Key
```http
GET /api/v1/auth/public-key/{tenantId}/{keyId}
```

### 3. JWKS Endpoint (OAuth2/OpenID Connect Standard)
```http
GET /api/v1/auth/.well-known/jwks/{tenantId}
```

## Integration with Other Microservices

### Step 1: Configure JWT Authentication

```csharp
// In other microservices
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

### Step 2: Implement Public Key Fetching

```csharp
public class PublicKeyService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public async Task<RSA> GetPublicKeyAsync(string tenantId, string keyId)
    {
        var cacheKey = $"publickey_{tenantId}_{keyId}";
        
        if (_cache.TryGetValue(cacheKey, out RSA cachedKey))
        {
            return cachedKey;
        }

        // Fetch from Authorization service
        var response = await _httpClient.GetAsync($"/api/v1/auth/public-key/{tenantId}/{keyId}");
        var publicKeyData = await response.Content.ReadFromJsonAsync<PublicKeyResponse>();
        
        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyData.PublicKeyPem);
        
        // Cache with expiration
        _cache.Set(cacheKey, rsa, publicKeyData.ExpiresAt);
        
        return rsa;
    }
}
```

## Security Considerations

1. **Key Rotation**: Keys automatically expire and rotate every 30 days
2. **Tenant Isolation**: Each tenant has separate key pairs
3. **Secure Storage**: Private keys are stored in distributed cache with expiration
4. **Algorithm Security**: Uses RSA-SHA256 (RS256) for strong cryptographic security

## Monitoring and Logging

The system logs key operations:
- Key pair generation
- Key rotation events
- Token generation (without sensitive data)
- Public key distribution requests

## Configuration

Required configuration in `appsettings.json`:

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

## Benefits

1. **Scalability**: Other services can validate tokens independently
2. **Security**: Private keys never leave the Authorization service
3. **Multi-tenancy**: Proper tenant isolation with separate key pairs
4. **Standards Compliance**: Follows JWT and OAuth2 best practices
5. **Key Management**: Automatic rotation and distribution