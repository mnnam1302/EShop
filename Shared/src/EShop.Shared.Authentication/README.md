# EShop.Shared.Authentication

Shared authentication library providing JWT token management, RSA key handling, and tenant-aware authentication for the EShop platform.

## Overview

This library provides:
- **JWT Token Generation & Validation** - Access and refresh token management
- **RSA Key Management** - Per-tenant RSA key pairs for token signing
- **Multi-tenant Support** - Tenant-aware key isolation and rotation

---

## Architecture

### High-Level Component Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        EShop.Shared.Authentication                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         Public Interfaces                            │   │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐   │   │
│  │  │ ITenantKeyProvider│  │   ITokenManager  │  │ITenantKeyCaching │   │   │
│  │  │                  │  │                  │  │    Service       │   │   │
│  │  └────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘   │   │
│  └───────────┼─────────────────────┼─────────────────────┼─────────────┘   │
│              │                     │                     │                 │
│  ┌───────────▼─────────────────────▼─────────────────────▼─────────────┐   │
│  │                       Implementations                                │   │
│  │                                                                      │   │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐   │   │
│  │  │ TenantKeyProvider│  │   TokenManager   │  │TenantKeyCaching  │   │   │
│  │  │                  │  │                  │  │    Service       │   │   │
│  │  │  - GetOrCreate   │  │  - GenerateToken │  │  - GetAsync      │   │   │
│  │  │  - GetKeyPair    │  │  - ValidateToken │  │  - AddAsync      │   │   │
│  │  │  - RotateKeyPair │  │  - RefreshToken  │  │  - SetActiveKey  │   │   │
│  │  │  - GetPreviousKey│  │                  │  │  - SetPreviousKey│   │   │
│  │  └────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘   │   │
│  └───────────┼─────────────────────┼─────────────────────┼─────────────┘   │
│              │                     │                     │                 │
│  ┌───────────▼─────────────────────▼─────────────────────▼─────────────┐   │
│  │                         Configuration                                │   │
│  │  ┌──────────────────┐              ┌──────────────────┐             │   │
│  │  │  TenantKeyOptions│              │    JwtOptions    │             │   │
│  │  │                  │              │                  │             │   │
│  │  │  - KeySizeInBits │              │  - Issuer        │             │   │
│  │  │  - KeyExpiryDays │              │  - Audience      │             │   │
│  │  │  - PreviousKeyTtl│              │  - AccessExpiry  │             │   │
│  │  └──────────────────┘              │  - RefreshExpiry │             │   │
│  │                                    └──────────────────┘             │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
                    ┌─────────────────────────────────┐
                    │         External Storage        │
                    │  ┌───────────┐  ┌───────────┐  │
                    │  │   Redis   │  │ In-Memory │  │
                    │  │  (Primary)│  │ (Fallback)│  │
                    │  └───────────┘  └───────────┘  │
                    └─────────────────────────────────┘
```

### Component Dependencies

```
┌──────────────────────────────────────────────────────────────────┐
│                    Dependency Flow                                │
└──────────────────────────────────────────────────────────────────┘

    ┌────────────────┐         ┌────────────────┐
    │  TokenManager  │────────►│   JwtOptions   │
    └───────┬────────┘         └────────────────┘
            │ depends on
            ▼
    ┌────────────────┐         ┌────────────────┐
    │TenantKeyProvider│────────►│TenantKeyOptions│
    └───────┬────────┘         │                │
            │ depends on       │ - KeySizeInBits│
            ▼                  │ - KeyExpiryDays│
    ┌────────────────┐         │ - PreviousKeyTtl│
    │TenantKeyCaching│         └────────────────┘
    │    Service     │
    └───────┬────────┘
            │
            ▼
    ┌────────────────┐
    │     Redis      │
    └────────────────┘
```

---

### Component Responsibilities

| Component | Responsibility | Configuration |
|-----------|----------------|---------------|
| `TenantKeyProvider` | RSA key pair lifecycle management (create, retrieve, rotate) | `TenantKeyOptions` |
| `TokenManager` | JWT token generation and validation | `JwtOptions` |
| `TenantKeyCachingService` | Redis-based key storage with in-memory fallback | - |

---

## Key Rotation Flow

### Sequence Diagram

```
┌─────────┐     ┌─────────────────┐     ┌─────────────────┐     ┌───────┐
│ Caller  │     │TenantKeyProvider│     │TenantKeyCaching │     │ Redis │
└────┬────┘     └────────┬────────┘     └────────┬────────┘     └───┬───┘
     │                   │                       │                  │
     │ RotateKeyPairAsync│                       │                  │
     │──────────────────>│                       │                  │
     │                   │                       │                  │
     │                   │ GetActiveKeyAsync     │                  │
     │                   │──────────────────────>│                  │
     │                   │                       │ GET active key   │
     │                   │                       │─────────────────>│
     │                   │                       │<─────────────────│
     │                   │<──────────────────────│                  │
     │                   │                       │                  │
     │                   │ Generate new RSA key  │                  │
     │                   │─────────┐             │                  │
     │                   │         │             │                  │
     │                   │<────────┘             │                  │
     │                   │                       │                  │
     │                   │ SetPreviousKeyAsync   │                  │
     │                   │ (TTL: PreviousKeyTtl) │                  │
     │                   │──────────────────────>│                  │
     │                   │                       │ SET previous key │
     │                   │                       │ (with TTL)       │
     │                   │                       │─────────────────>│
     │                   │                       │<─────────────────│
     │                   │<──────────────────────│                  │
     │                   │                       │                  │
     │                   │ SetActiveKeyAsync     │                  │
     │                   │──────────────────────>│                  │
     │                   │                       │ SET new active   │
     │                   │                       │─────────────────>│
     │                   │                       │<─────────────────│
     │                   │<──────────────────────│                  │
     │                   │                       │                  │
     │                   │ Update in-memory cache│                  │
     │                   │─────────┐             │                  │
     │                   │         │             │                  │
     │                   │<────────┘             │                  │
     │<──────────────────│                       │                  │
     │                   │                       │                  │
```

### Key State Timeline

```
Time ──────────────────────────────────────────────────────────────────────────>

BEFORE ROTATION:
┌─────────────────────────────────────────────────────────────────────────────┐
│  Active Key: KEY_A                                                          │
│  Previous Key: (none)                                                       │
│                                                                             │
│  Tokens signed with KEY_A ✓                                                 │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼ RotateKeyPairAsync()
                                    │
AFTER ROTATION:
┌─────────────────────────────────────────────────────────────────────────────┐
│  Active Key: KEY_B (new)                                                    │
│  Previous Key: KEY_A (TTL: PreviousKeyTtlMinutes)                          │
│                                                                             │
│  New tokens signed with KEY_B ✓                                             │
│  Old tokens (KEY_A) still valid ✓                                          │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼ After PreviousKeyTtlMinutes expires
                                    │
AFTER TTL EXPIRY:
┌─────────────────────────────────────────────────────────────────────────────┐
│  Active Key: KEY_B                                                          │
│  Previous Key: (expired/removed)                                            │
│                                                                             │
│  New tokens signed with KEY_B ✓                                             │
│  Old tokens (KEY_A) REJECTED ✗                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Token Validation Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Token Validation Process                            │
└─────────────────────────────────────────────────────────────────────────────┘

                        ┌────────────────┐
                        │ Incoming Token │
                        │   (JWT)        │
                        └───────┬────────┘
                                │
                                ▼
                    ┌───────────────────────┐
                    │ Extract KeyId (kid)   │
                    │ from token header     │
                    └───────────┬───────────┘
                                │
                                ▼
                    ┌───────────────────────┐
                    │ Get Active Key        │
                    │ for Tenant            │
                    └───────────┬───────────┘
                                │
                        ┌───────┴───────┐
                        │               │
                        ▼               ▼
               ┌─────────────┐  ┌─────────────────┐
               │ KeyId Match │  │ KeyId Mismatch  │
               │             │  │                 │
               └──────┬──────┘  └────────┬────────┘
                      │                  │
                      ▼                  ▼
              ┌───────────────┐  ┌─────────────────┐
              │ Validate with │  │ Get Previous Key│
              │ Active Key    │  │ for Tenant      │
              └───────┬───────┘  └────────┬────────┘
                      │                   │
                      │           ┌───────┴───────┐
                      │           │               │
                      │           ▼               ▼
                      │   ┌─────────────┐  ┌─────────────┐
                      │   │ KeyId Match │  │ No Previous │
                      │   │             │  │ Key / No    │
                      │   │             │  │ Match       │
                      │   └──────┬──────┘  └──────┬──────┘
                      │          │                │
                      │          ▼                ▼
                      │  ┌───────────────┐  ┌───────────────┐
                      │  │ Validate with │  │   REJECT      │
                      │  │ Previous Key  │  │   TOKEN       │
                      │  └───────┬───────┘  └───────────────┘
                      │          │
                      └────┬─────┘
                           │
                           ▼
                   ┌───────────────┐
                   │  VALID TOKEN  │
                   │  (Proceed)    │
                   └───────────────┘
```

---

## Redis Key Structure

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Redis Key Schema                                  │
└─────────────────────────────────────────────────────────────────────────────┘

Key Pattern: tenant:{tenantId}:rsa:{keyType}

Examples:
┌────────────────────────────────────┬────────────────────────────────────────┐
│ Redis Key                          │ Description                            │
├────────────────────────────────────┼────────────────────────────────────────┤
│ tenant:acme-corp:rsa:active        │ Current signing key for ACME Corp      │
│ tenant:acme-corp:rsa:previous      │ Previous key (with TTL) after rotation │
│ tenant:contoso:rsa:active          │ Current signing key for Contoso        │
│ tenant:contoso:rsa:previous        │ Previous key (with TTL) after rotation │
└────────────────────────────────────┴────────────────────────────────────────┘

Value Structure (RsaKeyPair):
┌─────────────────────────────────────────────────────────────────────────────┐
│ {                                                                           │
│   "KeyId": "550e8400-e29b-41d4-a716-446655440000",                         │
│   "TenantId": "acme-corp",                                                  │
│   "PrivateKeyPem": "-----BEGIN RSA PRIVATE KEY-----\n...",                 │
│   "PublicKeyPem": "-----BEGIN RSA PUBLIC KEY-----\n...",                   │
│   "CreatedAt": "2026-03-05T10:00:00Z",                                     │
│   "ExpiresAt": "2026-03-12T10:00:00Z"                                      │
│ }                                                                           │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Fallback Strategy

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Redis Failure Fallback Flow                              │
└─────────────────────────────────────────────────────────────────────────────┘

                    ┌────────────────────┐
                    │  GetKeyPairAsync   │
                    └─────────┬──────────┘
                              │
                              ▼
                    ┌────────────────────┐
                    │  Try Redis First   │
                    └─────────┬──────────┘
                              │
                    ┌─────────┴─────────┐
                    │                   │
                    ▼                   ▼
           ┌──────────────┐    ┌──────────────────┐
           │   Success    │    │  Redis Failed    │
           │              │    │  (Exception)     │
           └──────┬───────┘    └────────┬─────────┘
                  │                     │
                  │                     ▼
                  │            ┌──────────────────┐
                  │            │ Check In-Memory  │
                  │            │ Fallback Cache   │
                  │            └────────┬─────────┘
                  │                     │
                  │            ┌────────┴────────┐
                  │            │                 │
                  │            ▼                 ▼
                  │    ┌─────────────┐   ┌─────────────┐
                  │    │ Cache Hit   │   │ Cache Miss  │
                  │    │             │   │             │
                  │    └──────┬──────┘   └──────┬──────┘
                  │           │                 │
                  ▼           ▼                 ▼
           ┌─────────────────────┐      ┌─────────────┐
           │   Return KeyPair    │      │   THROW     │
           │   (Degraded Mode)   │      │  Exception  │
           └─────────────────────┘      └─────────────┘

In-Memory Cache Structure:
┌─────────────────────────────────────────────────────────────────┐
│ ConcurrentDictionary<string, RsaKeyPair>                        │
│                                                                 │
│ Key Format: "{tenantId}:active" or "{tenantId}:previous"       │
│                                                                 │
│ Examples:                                                       │
│ ├─ "acme-corp:active"    → RsaKeyPair                          │
│ ├─ "acme-corp:previous"  → RsaKeyPair                          │
│ ├─ "contoso:active"      → RsaKeyPair                          │
│ └─ "contoso:previous"    → RsaKeyPair                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## Configuration Classes

### `TenantKeyOptions`

Configuration for RSA key management:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `KeySizeInBits` | `int` | 2048 | RSA key size in bits |
| `KeyExpiryInDays` | `int` | 7 | Key validity period |
| `PreviousKeyTtlMinutes` | `int` | 15 | TTL for rotated keys during transition |

### `JwtOptions`

Configuration for JWT token behavior:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Issuer` | `string` | `http://authorization` | Token issuer claim |
| `Audience` | `string` | `http://authorization` | Token audience claim |
| `AccessTokenExpiryMinutes` | `int` | 15 | Access token lifetime |
| `RefreshTokenExpiryHours` | `int` | 1 | Refresh token lifetime |

---

## Usage

### Registration

```csharp
services.AddSharedAuthentication(configuration);
```

### Configuration (appsettings.json)

```json
{
  "TenantKey": {
    "KeySizeInBits": 2048,
    "KeyExpiryInDays": 7,
    "PreviousKeyTtlMinutes": 15
  },
  "Jwt": {
    "Issuer": "https://auth.eshop.com",
    "Audience": "https://api.eshop.com",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryHours": 1
  }
}
```

---

## File Structure

```
EShop.Shared.Authentication/
├── Abstractions/
│   ├── ITenantKeyProvider.cs       # Key management interface
│   ├── ITenantKeyCachingService.cs # Caching abstraction
│   └── ITokenManager.cs            # Token operations interface
├── DependencyInjections/
│   ├── JwtOptions.cs               # JWT configuration
│   ├── TenantKeyOptions.cs         # Key management configuration
│   └── ServiceCollectionExtensions.cs
├── Managers/
│   ├── RsaKey/
│   │   ├── TenantKeyProvider.cs    # Key lifecycle management
│   │   └── TenantKeyCachingService.cs
│   └── Token/
│       └── TokenManager.cs         # JWT generation/validation
├── Middlewares/
│   └── ...
├── RsaKeyPair.cs                   # Key pair model
└── README.md                       # This file
```
