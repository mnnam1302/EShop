## Why

The current authentication mechanism has critical security vulnerabilities (token cache collision, header spoofing), reliability risks (Redis single point of failure), and architectural friction (expensive service-to-service auth, fragile consumer patterns). As the platform grows with more microservices and developers, these issues compound — every new service inherits the same risks and every developer must understand auth internals to work safely. A centralized, standardized authentication architecture is needed now to establish a stable cross-cutting concern that lets developers focus on business logic.

## What Changes

- **Fix token cache collision**: Service-to-service calls via `SystemInternalJwtTokenFactory` overwrite the user's active session token in cache (keyed only by `userId`), causing concurrent user requests to fail authentication unexpectedly. Introduce scoped cache keys that separate external user sessions from internal S2S tokens.
- **Fix header spoofing vulnerability**: `HttpRequestUserDataProvider` trusts custom headers (`eshop-user-type`, `eshop-user-id`, `eshop-tenant-id`) without verifying request origin. External clients can send these headers to escalate privileges to system user. Headers must be validated against the JWT identity or stripped at the gateway.
- **Harden the auth handler**: `MultiTenantJwtBearerHandler.HandleAuthenticateAsync()` lacks try-catch around JWT validation calls that throw exceptions (`SecurityTokenException`, `InvalidOperationException`), leading to unhandled exceptions instead of proper `AuthenticateResult.Fail`.
- **Separate service-to-service auth from external auth**: Current S2S pattern generates a full JWT + 4 Redis round-trips per internal call. Introduce a lightweight internal authentication mechanism that avoids cache dependency and RSA operations for trusted internal traffic.
- **Standardize consumer auth context**: MassTransit consumers manually call `SetSystemUserContext`/`ClearSystemUserContext` with try/finally. Replace with a composable, `IDisposable`-based scope pattern that enforces cleanup.
- **Add auth pipeline resilience**: When Redis is unavailable, the entire platform becomes inaccessible (all tenants, all services). Add graceful degradation for cache failures.
- **Standardize error handling in auth pipeline**: The authentication pipeline mixes `Result<T>` pattern and thrown exceptions inconsistently across `UserTokenRedisCachingService` (throws `BadRequestException`), `JwtTokenManager` (throws `SecurityTokenException`), and `MultiTenantJwtBearerHandler` (uses `Result`). Unify to consistent error flow.
- **Implement RSA key rotation**: `TenantKeyProvider.RotateKeyPairAsync` is not implemented (`throw new NotImplementedException()`). Add graceful key rotation with overlap period for active tokens.

## Capabilities

### New Capabilities

- `auth-token-cache-isolation`: Cache key strategy that isolates external user sessions from service-to-service tokens, preventing token collision under concurrent load.
- `auth-header-security`: Validation rules ensuring custom identity headers cannot be spoofed by external clients, with gateway-level stripping or origin verification.
- `auth-handler-resilience`: Hardened authentication handler with consistent error handling, exception-safe JWT validation, and graceful degradation when infrastructure (Redis) is unavailable.
- `auth-service-to-service`: Lightweight internal authentication mechanism for trusted service-to-service communication that avoids full JWT generation and cache round-trips.
- `auth-consumer-scope`: Composable, `IDisposable`-based system user context scope for MassTransit consumers and background jobs, replacing manual try/finally patterns.
- `auth-key-rotation`: RSA key pair rotation for tenant signing keys with overlap period supporting active token validation during transition.

### Modified Capabilities

_(No existing specs are affected — all changes introduce new capabilities or modify implementation details of code not yet covered by specs.)_

## Impact

**Shared Libraries** (highest impact):
- `EShop.Shared.Authentication` — `MultiTenantJwtBearerHandler`, `HttpRequestUserDataProvider`, `SystemInternalJwtTokenFactory`, `JwtTokenManager`, `TenantKeyProvider`
- `EShop.Shared.Cache` — `UserTokenRedisCachingService`, cache key strategy
- `EShop.Shared.JsonApi` — `TenantAuthenticationExtensions`, authorization filter attributes
- `EShop.Shared.Scoping` — `UserOrganizationContextHttpClient`, `TenancyHttpClient` (S2S consumers)

**Services** (adopt improved patterns):
- `Authorization API` — Login/Refresh/Logout handlers, `TenantCreatedConsumer`
- `Tenancy API` — Feature service, system initialization
- `Catalog Application` — DB initializer, consumer patterns
- `API Gateway (ReverseProxy)` — Header stripping for external requests

**Test Infrastructure**:
- `EShop.Testing.JsonApiApplication` — `TestUserTokenCachingService`, `ApiTestContextBase` auth flow
- All BDD test setups across Authorization, Tenancy, Catalog

**Breaking Changes**: None at API contract level. All changes are internal to the shared authentication pipeline. Service code using `[RequireAuthenticatedUser]`, `[RequireSystemUser]`, `[RequirePermission]` attributes continues to work unchanged. Consumer auth pattern changes will require updating existing consumers but with a clear mechanical migration.
