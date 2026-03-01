## ADDED Requirements

### Requirement: Gateway strips internal identity headers from external requests
The API gateway (`EShop.ApiGateway`) SHALL remove all `eshop-*` custom headers from incoming external requests before forwarding them to backend services via YARP reverse proxy.

#### Scenario: External request with spoofed identity headers
- **WHEN** an external HTTP request arrives at the API gateway with header `eshop-user-type: SystemUser`
- **THEN** the gateway removes `eshop-user-type` before forwarding to the backend service
- **AND** the backend service does NOT receive the `eshop-user-type` header

#### Scenario: All internal identity headers are stripped
- **WHEN** an external HTTP request arrives at the API gateway with any combination of headers: `eshop-user-type`, `eshop-user-id`, `eshop-tenant-id`, `eshop-action-user-id`
- **THEN** all of these headers are removed before forwarding
- **AND** the backend service receives none of them

#### Scenario: Non-identity headers pass through
- **WHEN** an external HTTP request arrives with standard headers (`Authorization`, `Content-Type`, `Accept`) and custom non-identity headers (e.g., `X-Request-Id`)
- **THEN** those headers are forwarded unchanged to the backend service

### Requirement: Internal service-to-service headers are preserved
The header stripping SHALL only apply at the external gateway boundary. Service-to-service calls that bypass the gateway (direct internal HTTP) SHALL retain their custom headers.

#### Scenario: S2S call with identity headers reaches backend
- **WHEN** the Authorization service makes a direct HTTP call to the Tenancy service with headers `eshop-user-type`, `eshop-user-id`, `eshop-tenant-id`
- **THEN** the Tenancy service receives all custom headers intact
- **AND** `HttpRequestUserDataProvider` reads user context from those headers

### Requirement: Header stripping is implemented as YARP request transform
The header removal SHALL be implemented using YARP's built-in request transform configuration, not custom middleware. This ensures the stripping happens within YARP's processing pipeline before routing.

#### Scenario: YARP transform configuration
- **WHEN** the API gateway starts up with YARP reverse proxy configured
- **THEN** the YARP configuration includes request transforms that remove headers matching the pattern: `eshop-user-type`, `eshop-user-id`, `eshop-tenant-id`, `eshop-action-user-id`
