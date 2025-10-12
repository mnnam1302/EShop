# Complete Authentication Flow - Decoupled Microservices Architecture

## Overview
This document describes the complete user login flow and how other services can verify authentication even when the Authorization service is down, ensuring loose coupling and high availability.

## ?? User Login Flow

### Step 1: User Authenticates
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "john.doe@company.com",
  "password": "SecurePassword123!"
}
```

### Step 2: LoginQueryHandler Process
```
1. ? Validate credentials format
2. ?? Authenticate user against database
3. ?? Ensure RSA key pair exists for tenant (auto-generate if needed)
4. ?? Generate RSA-signed JWT tokens
5. ?? Cache tokens for session management
6. ? Return authentication response
```

### Step 3: Authentication Response
```json
{
  "userId": "user123",
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImtleS1pZC0xMjMifQ...",
  "refreshToken": "abc123def456...",
  "refreshTokenExpiryTime": "2024-01-15T10:30:00Z"
}
```

## ?? JWT Token Structure

### Header
```json
{
  "alg": "RS256",
  "typ": "JWT",
  "kid": "tenant-abc-key-123"
}
```

### Payload
```json
{
  "sub": "user123",
  "name": "John Doe",
  "tenant_id": "tenant-abc",
  "user_type": "user",
  "jti": "unique-token-id",
  "iat": 1640995200,
  "key_id": "tenant-abc-key-123",
  "exp": 1641081600,
  "iss": "EShop.Authorization",
  "aud": "EShop.Services"
}
```

### Signature
```
RSA-SHA256 signature using tenant's private key
```

## ?? Loose Coupling Architecture

### 1. Public Key Distribution Endpoints

Other services can fetch public keys independently:

```http
# Get active public key for tenant
GET /api/v1/auth/public-key/{tenantId}

# Get specific public key by ID
GET /api/v1/auth/public-key/{tenantId}/{keyId}

# JWKS endpoint (OAuth2 standard)
GET /api/v1/auth/.well-known/jwks/{tenantId}
```

### 2. Service Integration Pattern

Each microservice implements its own JWT validation:

```csharp
// In ApiGateway, Catalog, Order, etc.
public class JwtAuthenticationService
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _authServiceClient;
    private readonly ILogger<JwtAuthenticationService> _logger;

    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            
            // Extract tenant and key info
            var tenantId = jwt.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
            var keyId = jwt.Claims.FirstOrDefault(c => c.Type == "key_id")?.Value;
            
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(keyId))
                return null;

            // Get public key with caching and fallback
            var publicKey = await GetPublicKeyWithFallbackAsync(tenantId, keyId);
            if (publicKey == null)
                return null;

            // Validate token
            var validationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new RsaSecurityKey(publicKey) { KeyId = keyId },
                ValidateIssuer = true,
                ValidIssuer = "EShop.Authorization",
                ValidateAudience = true,
                ValidAudience = "EShop.Services",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    private async Task<RSA?> GetPublicKeyWithFallbackAsync(string tenantId, string keyId)
    {
        var cacheKey = $"publickey_{tenantId}_{keyId}";
        
        // Try cache first
        if (_cache.TryGetValue(cacheKey, out RSA cachedKey))
            return cachedKey;

        try
        {
            // Try to fetch from Authorization service
            var response = await _authServiceClient.GetAsync(
                $"/api/v1/auth/public-key/{tenantId}/{keyId}");
            
            if (response.IsSuccessStatusCode)
            {
                var keyData = await response.Content.ReadFromJsonAsync<PublicKeyResponse>();
                var rsa = RSA.Create();
                rsa.ImportFromPem(keyData.PublicKeyPem);
                
                // Cache with expiration
                _cache.Set(cacheKey, rsa, keyData.ExpiresAt.AddMinutes(-5)); // 5 min buffer
                return rsa;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch public key from Authorization service");
        }

        // Fallback: Try to get from local backup cache or shared storage
        return await GetPublicKeyFromBackupAsync(tenantId, keyId);
    }

    private async Task<RSA?> GetPublicKeyFromBackupAsync(string tenantId, string keyId)
    {
        // Implementation could include:
        // 1. Shared Redis cache between services
        // 2. Database backup of public keys
        // 3. File system cache
        // 4. Service mesh configuration
        
        try
        {
            // Example: Try Redis shared cache
            var backupKey = $"backup_publickey_{tenantId}_{keyId}";
            // var publicKeyPem = await _redisCache.GetStringAsync(backupKey);
            // if (!string.IsNullOrEmpty(publicKeyPem))
            // {
            //     var rsa = RSA.Create();
            //     rsa.ImportFromPem(publicKeyPem);
            //     return rsa;
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get public key from backup");
        }

        return null;
    }
}
```

### 3. API Gateway Configuration

```csharp
// In ApiGateway startup
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Extract token from Authorization header
                    var token = context.Request.Headers.Authorization
                        .FirstOrDefault()?.Parameter;
                    context.Token = token;
                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    // Additional custom validation if needed
                    var jwtService = context.HttpContext.RequestServices
                        .GetRequiredService<JwtAuthenticationService>();
                    
                    var principal = await jwtService.ValidateTokenAsync(context.SecurityToken.RawData);
                    if (principal != null)
                    {
                        context.Principal = principal;
                        context.Success();
                    }
                    else
                    {
                        context.Fail("Token validation failed");
                    }
                }
            };
        });
}
```

## ??? Resilience Strategies

### 1. Multi-Level Caching
```
1. Service Local Cache (MemoryCache) - 15 minutes
2. Shared Redis Cache - 1 hour
3. Database Backup - Persistent
4. Authorization Service - Source of truth
```

### 2. Circuit Breaker Pattern
```csharp
public class PublicKeyService
{
    private readonly CircuitBreakerPolicy _circuitBreaker;

    public PublicKeyService()
    {
        _circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromMinutes(1));
    }

    public async Task<RSA?> GetPublicKeyAsync(string tenantId, string keyId)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await FetchFromAuthorizationServiceAsync(tenantId, keyId);
            });
        }
        catch (CircuitBreakerOpenException)
        {
            // Fallback to cached or backup keys
            return await GetFromBackupAsync(tenantId, keyId);
        }
    }
}
```

### 3. Background Key Refresh
```csharp
public class PublicKeyRefreshService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Refresh keys for all active tenants
                await RefreshPublicKeysAsync();
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing public keys");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task RefreshPublicKeysAsync()
    {
        var activeTenants = await GetActiveTenantsAsync();
        
        foreach (var tenantId in activeTenants)
        {
            try
            {
                var publicKey = await FetchPublicKeyAsync(tenantId);
                if (publicKey != null)
                {
                    await StoreInBackupCacheAsync(tenantId, publicKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh key for tenant {TenantId}", tenantId);
            }
        }
    }
}
```

## ?? Benefits of This Architecture

### ? High Availability
- Services continue working even if Authorization service is down
- Multiple fallback mechanisms for key retrieval
- Background key refresh ensures fresh keys

### ? Performance
- Local caching reduces network calls
- JWT validation happens locally in each service
- No need to call Authorization service for every request

### ? Security
- RSA asymmetric encryption ensures strong security
- Private keys never leave Authorization service
- Tenant isolation with separate key pairs
- Automatic key rotation

### ? Scalability
- Each service validates tokens independently
- No centralized bottleneck
- Horizontal scaling without coordination

### ? Loose Coupling
- Services don't depend on Authorization service availability
- Standard JWT/OAuth2 protocols
- Can integrate with external identity providers

## ?? Token Refresh Flow

```csharp
// When access token expires, use refresh token
public async Task<AuthenticationResponse?> RefreshTokenAsync(string refreshToken)
{
    // 1. Validate refresh token from cache
    var cachedToken = await _tokenCachingService.GetTokenByRefreshAsync(refreshToken);
    if (cachedToken?.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        return null;

    // 2. Generate new access token
    var user = await _userRepository.GetByIdAsync(cachedToken.UserId);
    var newTokenResult = await GenerateAuthenticationTokensAsync(user);
    
    // 3. Update cache with new tokens
    await _tokenCachingService.UpdateTokenAsync(cachedToken.UserId, newTokenResult.Value);
    
    return newTokenResult.Value;
}
```

This architecture ensures your microservices ecosystem remains highly available and performant while maintaining strong security through RSA-signed JWT tokens.