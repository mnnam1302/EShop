## ADDED Requirements

### Requirement: S2S tokens use audience-scoped short-lived JWT
`SystemInternalJwtTokenFactory` SHALL generate JWTs with `audience = "internal"` and a maximum expiry of 30 seconds for all service-to-service calls. These tokens SHALL carry full user identity context (`userId`, `tenantId`, `userType`) in standard JWT claims.

#### Scenario: S2S token has internal audience claim
- **WHEN** `SystemInternalJwtTokenFactory.AddUserContext()` generates a token for a service-to-service call
- **THEN** the generated JWT contains claim `aud` with value `"internal"`
- **AND** the JWT is signed with the tenant's RSA private key

#### Scenario: S2S token expires in 30 seconds
- **WHEN** an internal JWT is generated at time T
- **THEN** the JWT `exp` claim is set to T + 30 seconds
- **AND** the token is rejected by the receiving service after 30 seconds have elapsed

#### Scenario: S2S token carries user identity
- **WHEN** `AddUserContext()` is called with `UserData { Id = "user-1", TenantId = "tenant-1", UserType = "TenantAdmin" }`
- **THEN** the generated JWT contains claims for `userId = "user-1"`, `tenantId = "tenant-1"`, and user type information
- **AND** the receiving service can extract identity from JWT claims

### Requirement: S2S tokens do not interact with Redis cache
`SystemInternalJwtTokenFactory` SHALL NOT call `IUserTokenCachingService.AddAsync()` and SHALL NOT generate refresh tokens for internal JWTs.

#### Scenario: No cache write during S2S token generation
- **WHEN** `SystemInternalJwtTokenFactory` generates an internal JWT
- **THEN** `IUserTokenCachingService.AddAsync()` is NOT invoked
- **AND** no refresh token is generated
- **AND** no `TokenAuthentication` object is created

#### Scenario: S2S call performance has zero Redis round-trips
- **WHEN** a service-to-service call is initiated and completed
- **THEN** the total number of Redis operations for authentication is zero on the calling side
- **AND** the total number of Redis operations for validation on the receiving side is zero (for internal audience tokens)

### Requirement: Auth handler skips cache validation for internal audience
`MultiTenantJwtBearerHandler` SHALL check the `audience` claim after successful JWT signature verification. If the audience is `"internal"`, the handler SHALL skip `ValidateTokenInCacheAsync()` and return success immediately.

#### Scenario: Internal JWT bypasses cache validation
- **WHEN** a request arrives with a Bearer token containing `aud = "internal"`
- **AND** the JWT signature is valid (RSA verification passes)
- **AND** the JWT is not expired
- **THEN** the handler does NOT call `ValidateTokenInCacheAsync()`
- **AND** the handler returns `AuthenticateResult.Success`

#### Scenario: External JWT still requires cache validation
- **WHEN** a request arrives with a Bearer token that does NOT contain `aud = "internal"`
- **AND** the JWT signature is valid
- **THEN** the handler calls `ValidateTokenInCacheAsync()` as before
- **AND** the token MUST match the cached token to authenticate successfully

#### Scenario: Expired internal JWT is rejected
- **WHEN** a request arrives with a Bearer token containing `aud = "internal"`
- **AND** the JWT `exp` claim indicates the token has expired
- **THEN** the handler returns `AuthenticateResult.Fail`
- **AND** the request is rejected with 401

### Requirement: S2S calls continue using Bearer authentication scheme
Internal JWTs SHALL use the standard `Authorization: Bearer <jwt>` header. No new authentication schemes or handlers are introduced.

#### Scenario: S2S request uses Bearer scheme
- **WHEN** `SystemInternalJwtTokenFactory.AddUserContext()` sets the authorization header on an HttpClient
- **THEN** the header value is `Bearer <jwt>` where `<jwt>` is the short-lived internal token
- **AND** the receiving service's `MultiTenantJwtBearerHandler` processes it through the existing Bearer scheme

### Requirement: Custom identity headers remain for HttpRequestUserDataProvider compatibility
`SystemInternalJwtTokenFactory.AddUserContext()` SHALL continue to set custom headers (`eshop-user-type`, `eshop-user-id`, `eshop-tenant-id`, `eshop-action-user-id`) on S2S HttpClient requests for backward compatibility with `HttpRequestUserDataProvider`.

#### Scenario: S2S call includes both JWT and custom headers
- **WHEN** `AddUserContext()` prepares an HttpClient for a S2S call with a user of type `AppClientWithIndividualUsers`
- **THEN** the request includes `Authorization: Bearer <internal-jwt>` header
- **AND** the request includes `eshop-user-type`, `eshop-user-id`, `eshop-tenant-id`, `eshop-action-user-id` custom headers
