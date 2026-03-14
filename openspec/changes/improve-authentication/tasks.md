## 1. Gateway Header Stripping (D2 — Phase 1)

- [x] 1.1 Add YARP request transforms in `EShop.ApiGateway` to remove `eshop-user-type`, `eshop-user-id`, `eshop-tenant-id`, `eshop-action-user-id` headers from incoming external requests
- [ ] 1.2 Write integration test: external request with spoofed `eshop-user-type: SystemUser` header is stripped before reaching backend
- [ ] 1.3 Write integration test: standard headers (`Authorization`, `Content-Type`, `X-Request-Id`) pass through unchanged
- [ ] 1.4 Write integration test: direct S2S call (bypassing gateway) retains all custom identity headers

## 2. Auth Handler Hardening (D3 — Phase 1)

- [x] 2.1 Wrap `MultiTenantJwtBearerHandler.HandleAuthenticateAsync()` body in try-catch, converting all exceptions to `AuthenticateResult.Fail` with descriptive messages
- [x] 2.2 Add Warning-level logging for caught exceptions inside the handler
- [ ] 2.3 Write unit test: `SecurityTokenExpiredException` from `GetPrincipalFromTokenAsync` → `AuthenticateResult.Fail` (not 500)
- [ ] 2.4 Write unit test: `SecurityTokenException` (malformed JWT) → `AuthenticateResult.Fail`
- [ ] 2.5 Write unit test: `InvalidOperationException` (missing tenant RSA key) → `AuthenticateResult.Fail`
- [ ] 2.6 Write unit test: unexpected `Exception` type → `AuthenticateResult.Fail` with logged warning

## 3. Token Cache Isolation (D1 — Phase 1)

- [x] 3.1 Modify `UserTokenCacheKeyProvider.GetCacheKey()` to return `authorization:tokens:user:{userId}` (add `user:` scope prefix)
- [x] 3.2 Update `LoginQueryHandler` (or equivalent login flow) to write session tokens using the new scoped cache key
- [x] 3.3 Update `MultiTenantJwtBearerHandler.ValidateTokenInCacheAsync()` to check scoped key `authorization:tokens:user:{userId}` first, then fallback to legacy key `authorization:tokens:{userId}` for migration
- [ ] 3.4 Write unit test: login writes to `authorization:tokens:user:{userId}`, not legacy key
- [ ] 3.5 Write unit test: validation finds token under new scoped key
- [ ] 3.6 Write unit test: validation falls back to legacy key and succeeds (migration scenario)
- [ ] 3.7 Write unit test: concurrent S2S call does not evict a user's cached session

## 4. S2S Short-Lived Internal JWT (D4 — Phase 2)

- [x] 4.1 Refactor `SystemInternalJwtTokenFactory.GenerateToken()` to add `audience = "internal"` claim and set expiry to 30 seconds
- [x] 4.2 Remove `userTokenCachingService.AddAsync()` call and refresh token generation from `SystemInternalJwtTokenFactory.GenerateToken()`
- [x] 4.3 Remove `IUserTokenCachingService` dependency from `SystemInternalJwtTokenFactory` constructor
- [x] 4.4 Add audience check in `MultiTenantJwtBearerHandler.HandleAuthenticateAsync()`: after `GetPrincipalFromTokenAsync`, if `aud = "internal"`, skip `ValidateTokenInCacheAsync()` and return success
- [ ] 4.5 Write unit test: `SystemInternalJwtTokenFactory` generates JWT with `aud = "internal"` and 30s expiry
- [ ] 4.6 Write unit test: `SystemInternalJwtTokenFactory` does NOT call `IUserTokenCachingService.AddAsync()`
- [ ] 4.7 Write unit test: S2S generated JWT carries correct `userId`, `tenantId`, and user type claims
- [ ] 4.8 Write unit test: handler skips cache validation for token with `aud = "internal"` and valid signature
- [ ] 4.9 Write unit test: handler still performs cache validation for external token (no `aud = "internal"`)
- [ ] 4.10 Write unit test: expired internal JWT (past 30s) is rejected with `AuthenticateResult.Fail`
- [ ] 4.11 Update `ApiTestContextBase` in test infrastructure to add `audience = "internal"` claim for S2S test tokens

## 5. Consumer Auth Scope (D5 — Phase 2)

- [x] 5.1 Create `SystemUserContextConsumeFilter<TMessage>` class in `EShop.Shared.Authentication` implementing `IFilter<ConsumeContext<TMessage>>` with `where TMessage : class, IIntegrationEvent`
- [x] 5.2 Implement `Send()` method: extract `TenantId`, `ActionUserId`, `ActionUserType` from message, call `SetSystemUserContext()`, delegate to next pipe in try/finally with `ClearSystemUserContext()` in finally
- [x] 5.3 Create `SystemUserScope : IDisposable` class that wraps `SetSystemUserContext` on creation and `ClearSystemUserContext` on `Dispose()`
- [x] 5.4 Add `CreateSystemUserScope(string tenantId, string? userId = null, string? userType = null)` method to `IUserDetailsProvider` interface
- [x] 5.5 Implement `CreateSystemUserScope()` in `HttpRequestUserDataProvider`
- [x] 5.6 Register `SystemUserContextConsumeFilter<>` in Authorization service MassTransit configuration using `UseConsumeFilter` with `IIntegrationEvent` include predicate
- [x] 5.7 Register `SystemUserContextConsumeFilter<>` in Tenancy service MassTransit configuration
- [x] 5.8 Register `SystemUserContextConsumeFilter<>` in Catalog service MassTransit configuration (if applicable)
- [ ] 5.9 Write unit test: `SystemUserContextConsumeFilter` calls `SetSystemUserContext` with correct values from `IIntegrationEvent` message
- [ ] 5.10 Write unit test: `SystemUserContextConsumeFilter` calls `ClearSystemUserContext` in finally block (even on exception)
- [ ] 5.11 Write unit test: `SystemUserScope` calls `ClearSystemUserContext` on dispose
- [ ] 5.12 Write unit test: double-clear (filter + manual consumer code) does not throw errors

## 6. Migrate Existing Consumers and Services to New Scope Pattern (D5 — Phase 2)

- [x] 6.1 Remove manual `SetSystemUserContext`/`ClearSystemUserContext` from `TenantCreatedConsumer` in Authorization service
- [x] 6.2 Replace manual try/finally in `DbInitializer` (Tenancy API) with `using var scope = userDetailsProvider.CreateSystemUserScope()`
- [x] 6.3 Replace manual try/finally in `FeatureService` (Tenancy Application) with `using var scope` — all 4 occurrences
- [x] 6.4 Replace manual try/finally in `SystemInitializationService` (Tenancy Application) with `using var scope`
- [x] 6.5 Replace manual try/finally in `EnableTenantFeatureCommandHandler` (Tenancy Application) with `using var scope`
- [x] 6.6 Replace manual try/finally in `CreateTenantCommandHandler` (Tenancy Application) with `using var scope`
- [ ] 6.7 Verify all BDD tests pass after consumer migration (Authorization, Tenancy, Catalog test suites)

## 7. Auth Pipeline Resilience (D6 — Phase 2)

- [x] 7.1 Modify `UserTokenRedisCachingService.GetAsync()` to catch Redis exceptions, log at Warning level, and return `null` instead of throwing `BadRequestException`
- [x] 7.2 Modify `UserTokenRedisCachingService.GetAsync()` to return `null` on cache miss instead of throwing
- [x] 7.3 Update `MultiTenantJwtBearerHandler.ValidateTokenInCacheAsync()` to treat `null` cache result as degraded mode — skip cache validation, log warning, return success if JWT signature is valid
- [x] 7.4 Add in-memory `ConcurrentDictionary` fallback to `TenantKeyProvider` for RSA key pairs loaded during the current process lifetime
- [x] 7.5 Update `TenantKeyProvider.GetOrCreateKeyPairAsync()` to write to in-memory cache on successful Redis read and fall back to in-memory cache on Redis failure
- [ ] 7.6 Write unit test: `UserTokenRedisCachingService.GetAsync()` returns `null` when Redis throws `RedisConnectionException`
- [ ] 7.7 Write unit test: `UserTokenRedisCachingService.GetAsync()` returns `null` on cache miss (no exception)
- [ ] 7.8 Write unit test: handler authenticates successfully in degraded mode (valid JWT, Redis unavailable)
- [ ] 7.9 Write unit test: handler rejects invalid JWT even in degraded mode
- [ ] 7.10 Write unit test: `TenantKeyProvider` returns key from in-memory fallback when Redis is unavailable
- [ ] 7.11 Write unit test: `TenantKeyProvider` returns failure for never-loaded tenant key when Redis is unavailable

## 8. RSA Key Rotation (D7 — Phase 3)

- [x] 8.1 Update `TenantKeyProvider` cache key structure to use `authorization:keys:{tenantId}:active` and `authorization:keys:{tenantId}:previous`
- [x] 8.2 Implement `RotateKeyPairAsync()` in `TenantKeyProvider`: generate new key pair, move current active to previous with TTL = `AccessTokenExpiryMinutes`, store new as active
- [x] 8.3 Add `kid` (Key ID) header to JWTs generated by `JwtTokenManager.GenerateAccessToken()`
- [x] 8.4 Update `JwtTokenManager.GetPrincipalFromTokenAsync()` to try active key first, then fall back to previous key if signature fails and `kid` matches
- [x] 8.5 Update in-memory key cache (from D6) to store both active and previous keys on rotation
- [x] 8.6 Write unit test: `RotateKeyPairAsync` no longer throws `NotImplementedException`
- [x] 8.7 Write unit test: after rotation, new tokens are signed with the new active key
- [x] 8.8 Write unit test: after rotation, tokens signed with the previous key still validate via fallback
- [x] 8.9 Write unit test: after previous key TTL expires, tokens signed with it are rejected
- [x] 8.10 Write unit test: in-memory cache contains both active and previous keys after rotation
- [x] 8.11 Write unit test: first rotation (no existing previous key) completes without error
