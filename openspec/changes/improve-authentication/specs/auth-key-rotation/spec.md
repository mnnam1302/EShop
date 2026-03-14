## ADDED Requirements

### Requirement: Key rotation generates new key pair and preserves previous
`TenantKeyProvider.RotateKeyPairAsync()` SHALL generate a new RSA key pair, store it as the "active" key, and move the current active key to "previous" with an expiry matching the access token TTL.

#### Scenario: Successful key rotation
- **WHEN** `RotateKeyPairAsync` is called for tenant T1
- **THEN** a new RSA key pair is generated
- **AND** the new key pair is stored in Redis under `authorization:keys:{tenantId}:active`
- **AND** the previous active key pair is moved to `authorization:keys:{tenantId}:previous`
- **AND** the previous key has a TTL equal to the configured `JwtOptions.AccessTokenExpiryMinutes`

#### Scenario: First rotation when no previous key exists
- **WHEN** `RotateKeyPairAsync` is called for a tenant that has an active key but no previous key
- **THEN** the current active key becomes the previous key
- **AND** a new active key is generated
- **AND** no error occurs from the missing previous key

### Requirement: Token signing always uses active key
`JwtTokenManager.GenerateAccessToken()` SHALL always sign tokens with the "active" RSA key pair for the tenant.

#### Scenario: New token is signed with active key
- **WHEN** a JWT access token is generated for tenant T1
- **THEN** the token is signed with the RSA private key from `authorization:keys:{tenantId}:active`
- **AND** the token's `kid` (Key ID) header identifies which key was used

### Requirement: Token validation falls back to previous key
`JwtTokenManager.GetPrincipalFromTokenAsync()` SHALL attempt validation with the "active" key first. If validation fails due to signature mismatch, it SHALL retry with the "previous" key if one exists and the token's `kid` matches.

#### Scenario: Token signed with active key validates immediately
- **WHEN** a JWT is presented that was signed with the current active key
- **THEN** validation succeeds on the first attempt using the active key
- **AND** no fallback to the previous key is attempted

#### Scenario: Token signed with previous key validates via fallback
- **WHEN** a key rotation has occurred for tenant T1
- **AND** a JWT signed with the previous (now rotated-out) key is presented
- **AND** the token has not expired
- **THEN** validation fails with the active key (signature mismatch)
- **AND** validation succeeds with the previous key
- **AND** `AuthenticateResult.Success` is returned

#### Scenario: Token signed with expired previous key is rejected
- **WHEN** a key rotation occurred long enough ago that the previous key has expired from cache
- **AND** a JWT signed with that expired previous key is presented
- **THEN** validation fails with the active key
- **AND** no previous key exists in cache to fall back to
- **AND** `AuthenticateResult.Fail` is returned

### Requirement: Previous key auto-expires from cache
The previous RSA key pair SHALL have a Redis TTL equal to the configured access token expiry duration. After this TTL, the key is automatically evicted and tokens signed with it will no longer validate.

#### Scenario: Previous key TTL matches access token expiry
- **WHEN** a key rotation occurs and `JwtOptions.AccessTokenExpiryMinutes` is configured to 60
- **THEN** the previous key is stored with a Redis TTL of 60 minutes
- **AND** after 60 minutes the previous key is no longer in Redis

### Requirement: RotateKeyPairAsync replaces NotImplementedException
The current `TenantKeyProvider.RotateKeyPairAsync()` that throws `NotImplementedException` SHALL be replaced with a working implementation.

#### Scenario: RotateKeyPairAsync no longer throws
- **WHEN** `RotateKeyPairAsync` is called
- **THEN** it does NOT throw `NotImplementedException`
- **AND** it completes the key rotation successfully
- **AND** it returns a success result

### Requirement: In-memory cache is updated on rotation
When a key rotation occurs, the in-memory key cache (used as fallback per auth-handler-resilience) SHALL be updated to include both the new active key and the previous key.

#### Scenario: In-memory cache reflects rotated keys
- **WHEN** a key rotation occurs for tenant T1
- **THEN** the in-memory cache for T1 is updated with the new active key pair
- **AND** the in-memory cache retains the previous key pair
- **AND** subsequent JWT validations use the updated keys without requiring Redis access
