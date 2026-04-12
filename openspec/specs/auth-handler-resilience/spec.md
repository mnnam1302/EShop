## ADDED Requirements

### Requirement: Auth handler catches all JWT validation exceptions
`MultiTenantJwtBearerHandler.HandleAuthenticateAsync()` SHALL wrap the JWT validation pipeline in a try-catch block. All exceptions thrown by `IJwtTokenManager.GetPrincipalFromTokenAsync()` SHALL be converted to `AuthenticateResult.Fail` with a descriptive error message.

#### Scenario: SecurityTokenExpiredException produces 401
- **WHEN** a request contains an expired JWT access token
- **AND** `GetPrincipalFromTokenAsync` throws `SecurityTokenExpiredException`
- **THEN** the handler returns `AuthenticateResult.Fail` with message indicating token expiration
- **AND** the HTTP response status code is 401

#### Scenario: SecurityTokenException produces 401
- **WHEN** a request contains a malformed or invalid JWT
- **AND** `GetPrincipalFromTokenAsync` throws `SecurityTokenException`
- **THEN** the handler returns `AuthenticateResult.Fail` with message indicating invalid token
- **AND** the HTTP response status code is 401

#### Scenario: InvalidOperationException produces 401
- **WHEN** a request contains a JWT that cannot be processed due to missing tenant RSA key
- **AND** `GetPrincipalFromTokenAsync` throws `InvalidOperationException`
- **THEN** the handler returns `AuthenticateResult.Fail` with a generic authentication failure message
- **AND** the HTTP response status code is 401

#### Scenario: Unhandled exception produces 401 not 500
- **WHEN** any unexpected exception occurs during JWT validation
- **THEN** the handler catches it and returns `AuthenticateResult.Fail`
- **AND** the exception is logged at Warning level
- **AND** the HTTP response status code is 401, not 500

### Requirement: Auth handler degrades gracefully when Redis is unavailable
When `IUserTokenCachingService` is unable to reach Redis, the auth handler SHALL skip cache validation and rely on JWT signature validation alone (degraded mode).

#### Scenario: Redis connection failure allows authenticated request
- **WHEN** a request contains a valid JWT (correct RSA signature, not expired)
- **AND** Redis is unreachable (connection timeout or exception)
- **THEN** the handler skips cache validation
- **AND** the handler returns `AuthenticateResult.Success` based on JWT signature alone
- **AND** a warning is logged indicating degraded authentication mode

#### Scenario: Invalid JWT is still rejected during Redis outage
- **WHEN** a request contains an invalid JWT (bad signature or expired)
- **AND** Redis is unreachable
- **THEN** the handler returns `AuthenticateResult.Fail`
- **AND** the request is rejected with 401

### Requirement: Token caching service does not throw on cache miss or failure
`UserTokenRedisCachingService.GetAsync()` SHALL return `null` when the cache key does not exist or when Redis is unreachable. It SHALL NOT throw `BadRequestException` or any other exception for operational cache failures.

#### Scenario: Cache miss returns null
- **WHEN** `GetAsync` is called with a `userId` that has no cached token
- **THEN** the method returns `null`
- **AND** no exception is thrown

#### Scenario: Redis connection failure returns null
- **WHEN** `GetAsync` is called while Redis is unreachable
- **THEN** the method returns `null`
- **AND** the Redis exception is logged at Warning level
- **AND** no exception propagates to the caller

### Requirement: TenantKeyProvider uses in-memory fallback for loaded keys
When Redis is unavailable, `TenantKeyProvider` SHALL serve RSA key pairs from an in-memory cache for keys that were previously loaded in the current process lifetime.

#### Scenario: Previously loaded key is served from memory during Redis outage
- **WHEN** tenant T1's RSA key pair was loaded from Redis during normal operation
- **AND** Redis becomes unreachable
- **AND** a request for tenant T1's key pair is made
- **THEN** the key pair is returned from the in-memory fallback
- **AND** JWT signing/verification continues to work for tenant T1

#### Scenario: Never-loaded key is unavailable during Redis outage
- **WHEN** tenant T2's RSA key pair was never loaded from Redis
- **AND** Redis is unreachable
- **AND** a request for tenant T2's key pair is made
- **THEN** the method returns a failure result
- **AND** authentication fails for tenant T2 with a descriptive error
