# rate-limit-policy-management

## ADDED Requirements

### Requirement: Tenant rate-limit policy model
The Tenancy service SHALL persist an optional rate-limit policy as part of a tenant's settings, consisting of rules with a route-group domain (`tenancy`, `authorization`, `catalog`, or `*`), a scope (`Tenant`, `User`, or `AnonymousIp`), a time unit (`Second`, `Minute`, `Hour`, `Day`), a requests-per-unit value, and an optional burst capacity.

#### Scenario: Policy with rules is persisted and retrievable
- **WHEN** a policy containing the rule `{ domain: "*", scope: User, unit: Minute, requestsPerUnit: 120, burst: 150 }` is set on a tenant
- **THEN** reading the tenant's settings returns the policy with that rule intact

#### Scenario: Tenant without a policy
- **WHEN** a tenant's settings contain no rate-limit policy
- **THEN** the tenant is treated as inheriting platform defaults and no rule data is stored for it

### Requirement: Policy validation invariants
The Tenancy service SHALL reject a rate-limit policy that violates any invariant: `requestsPerUnit` MUST be greater than zero; `burst`, when set, MUST be greater than or equal to `requestsPerUnit`; `(domain, scope)` pairs MUST be unique within a policy; the rule count MUST NOT exceed the configured maximum.

#### Scenario: Non-positive rate rejected
- **WHEN** a policy is submitted containing a rule with `requestsPerUnit: 0`
- **THEN** the policy is rejected with a domain validation error and no changes are persisted

#### Scenario: Duplicate domain and scope rejected
- **WHEN** a policy is submitted containing two rules with the same `(domain, scope)` pair
- **THEN** the policy is rejected with a domain validation error

#### Scenario: Burst below sustained rate rejected
- **WHEN** a policy is submitted containing a rule with `burst: 5` and `requestsPerUnit: 100`
- **THEN** the policy is rejected with a domain validation error

### Requirement: Platform defaults on the system tenant
The platform's default rate-limit rules — including an `AnonymousIp` rule for the `authorization` domain — SHALL be stored as the system tenant's rate-limit policy, and customer tenants SHALL NOT be seeded with rate-limit rules at creation.

#### Scenario: System tenant holds the login rule
- **WHEN** the system tenant's rate-limit policy is read
- **THEN** it contains a rule with `domain: "authorization"`, `scope: AnonymousIp`, and a per-minute limit

#### Scenario: New tenant stores no rules
- **WHEN** a new tenant is created with default settings
- **THEN** its settings contain no rate-limit policy

### Requirement: Restricted policy administration
The Tenancy service SHALL expose an endpoint to set a tenant's rate-limit policy, accessible only to system/support users; tenant users MUST NOT be able to modify their own tenant's policy.

#### Scenario: System admin updates a tenant's policy
- **WHEN** a system/support user submits a valid policy for a tenant
- **THEN** the policy is persisted and returned by subsequent reads

#### Scenario: Tenant user denied
- **WHEN** an authenticated user of the target tenant (without system/support privileges) attempts to set the tenant's policy
- **THEN** the request is rejected as forbidden and the stored policy is unchanged

### Requirement: Internal policy read endpoint
The Tenancy service SHALL expose an internal service-to-service endpoint returning a tenant's stored rate-limit policy, resolving the tenant regardless of caller scope, and distinguishing "tenant has no policy" from "tenant not found".

#### Scenario: Tenant with a policy
- **WHEN** the internal endpoint is called for a tenant that has a stored policy
- **THEN** the response contains the policy's rules

#### Scenario: Tenant without a policy
- **WHEN** the internal endpoint is called for an existing tenant that has no stored policy
- **THEN** the response explicitly indicates absence of a policy (enabling negative caching) rather than an error
