## ADDED Requirements

### Requirement: User session cache keys include scope prefix
The `UserTokenCacheKeyProvider` SHALL generate cache keys with a `user:` scope prefix for external user sessions. The key format SHALL be `authorization:tokens:user:{userId}`.

#### Scenario: External user login writes to scoped cache key
- **WHEN** a user authenticates via the login endpoint
- **THEN** the session token is stored in Redis under `authorization:tokens:user:{userId}`
- **AND** the key does NOT use the legacy format `authorization:tokens:{userId}`

#### Scenario: External user token validation reads from scoped cache key
- **WHEN** `MultiTenantJwtBearerHandler` validates an external user's access token
- **THEN** the handler looks up the cached token using key `authorization:tokens:user:{userId}`

### Requirement: S2S tokens do not write to the user session cache
The `SystemInternalJwtTokenFactory` SHALL NOT write tokens to the user token cache. S2S tokens (with `audience = "internal"`) are validated by RSA signature and short TTL only, with no cache interaction.

#### Scenario: S2S call does not create a cache entry
- **WHEN** `SystemInternalJwtTokenFactory.AddUserContext()` generates an internal JWT for a service-to-service call
- **THEN** no call is made to `IUserTokenCachingService.AddAsync()`
- **AND** no refresh token is generated

#### Scenario: Concurrent S2S call does not evict user session
- **WHEN** User A is logged in with an active session cached under `authorization:tokens:user:{userA-id}`
- **AND** a service-to-service call is made on behalf of User A
- **THEN** User A's cached session token remains unchanged
- **AND** User A's subsequent API requests continue to authenticate successfully

### Requirement: Backward-compatible cache key migration
During rolling deployment, `MultiTenantJwtBearerHandler` SHALL check both the new scoped cache key format (`authorization:tokens:user:{userId}`) and the legacy format (`authorization:tokens:{userId}`) when validating external user tokens.

#### Scenario: Token cached under legacy key still validates
- **WHEN** a user's token was cached under the legacy key `authorization:tokens:{userId}` before deployment
- **AND** the user sends a request after the new handler is deployed
- **THEN** the handler finds and validates the token from the legacy key
- **AND** authentication succeeds

#### Scenario: Legacy fallback is removed after full deployment
- **WHEN** all services have been deployed with the new cache key format
- **THEN** the legacy key lookup path SHALL be removable via a configuration flag or code cleanup without behavioral change
