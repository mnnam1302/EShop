## ADDED Requirements

### Requirement: MassTransit ConsumeFilter auto-sets system user context for integration events
A scoped `IFilter<ConsumeContext<TMessage>>` named `SystemUserContextConsumeFilter<TMessage>` SHALL be registered in the MassTransit pipeline. For any message implementing `IIntegrationEvent`, the filter SHALL extract `TenantId`, `ActionUserId`, and `ActionUserType` from the message and call `IUserDetailsProvider.SetSystemUserContext()` before the consumer executes.

#### Scenario: Consumer receives pre-configured auth context
- **WHEN** a MassTransit consumer receives an `IIntegrationEvent` message with `TenantId = "tenant-1"`, `ActionUserId = "user-1"`, `ActionUserType = "SystemUser"`
- **THEN** `IUserDetailsProvider.SetSystemUserContext("tenant-1", "user-1", "SystemUser")` is called before the consumer's `Consume()` method executes
- **AND** the consumer can access `IUserDetailsProvider.AuthenticatedUser` with the correct tenant and user context

#### Scenario: Consumer does not contain auth setup code
- **WHEN** a MassTransit consumer class implements `IConsumer<TMessage>` where `TMessage : IIntegrationEvent`
- **THEN** the consumer's `Consume()` method SHALL NOT call `SetSystemUserContext()` or `ClearSystemUserContext()`
- **AND** the consumer contains only business logic

### Requirement: ConsumeFilter cleans up auth context after consumer execution
The `SystemUserContextConsumeFilter` SHALL call `IUserDetailsProvider.ClearSystemUserContext()` in a `finally` block after the consumer pipeline completes, regardless of success or failure.

#### Scenario: Auth context is cleared after successful consumer execution
- **WHEN** a consumer processes an `IIntegrationEvent` message successfully
- **THEN** `ClearSystemUserContext()` is called after the consumer returns

#### Scenario: Auth context is cleared after consumer exception
- **WHEN** a consumer throws an exception during `Consume()`
- **THEN** `ClearSystemUserContext()` is still called in the `finally` block
- **AND** the exception propagates to MassTransit's retry/error pipeline

### Requirement: ConsumeFilter is registered once per service
The `SystemUserContextConsumeFilter<>` SHALL be registered in each service's MassTransit configuration using `UseConsumeFilter` with an include predicate that targets messages implementing `IIntegrationEvent`. It SHALL NOT require per-consumer registration.

#### Scenario: Filter applies to all IIntegrationEvent consumers automatically
- **WHEN** a new consumer for `INewFeatureEnabled` (which implements `IIntegrationEvent`) is added to a service
- **THEN** the `SystemUserContextConsumeFilter` automatically applies to it without any additional configuration
- **AND** the developer does NOT need to register the filter for the new consumer

#### Scenario: Filter does not apply to non-IIntegrationEvent messages
- **WHEN** a consumer handles a message type that does NOT implement `IIntegrationEvent`
- **THEN** the `SystemUserContextConsumeFilter` is NOT invoked
- **AND** no auth context is set or cleared

### Requirement: ConsumeFilter resolves from DI with scoped lifetime
The `SystemUserContextConsumeFilter` SHALL be resolved from the DI container with scoped lifetime per-message, matching the lifetime of `IUserDetailsProvider`. MassTransit's scoped filter infrastructure SHALL handle the DI scope creation.

#### Scenario: Each message gets its own filter instance
- **WHEN** two messages are processed concurrently by the same consumer type
- **THEN** each message gets a separate `SystemUserContextConsumeFilter` instance
- **AND** each instance has its own scoped `IUserDetailsProvider`
- **AND** the auth contexts do not interfere with each other

### Requirement: IDisposable SystemUserScope for non-consumer contexts
`IUserDetailsProvider` SHALL expose a `CreateSystemUserScope(string tenantId, string? userId = null, string? userType = null)` method that returns a `SystemUserScope : IDisposable`. The scope calls `SetSystemUserContext()` on creation and `ClearSystemUserContext()` on dispose.

#### Scenario: Using statement auto-clears context
- **WHEN** code creates a scope via `using var scope = userDetailsProvider.CreateSystemUserScope("tenant-1")`
- **AND** the `using` block completes (normally or via exception)
- **THEN** `ClearSystemUserContext()` is called automatically on dispose

#### Scenario: DbInitializer uses SystemUserScope
- **WHEN** a `DbInitializer` needs system user context to seed data for a tenant
- **THEN** it creates a scope via `using var scope = userDetailsProvider.CreateSystemUserScope(tenantId)`
- **AND** all operations within the using block execute with the correct tenant context
- **AND** the context is auto-cleared when the block exits

#### Scenario: Hangfire job uses SystemUserScope
- **WHEN** a Hangfire background job needs to execute on behalf of a tenant
- **THEN** it creates a scope via `using var scope = userDetailsProvider.CreateSystemUserScope(tenantId)`
- **AND** the scope provides the same auth context behavior as the MassTransit ConsumeFilter

### Requirement: Backward compatibility during migration
The existing `SetSystemUserContext()` and `ClearSystemUserContext()` methods SHALL remain available on `IUserDetailsProvider`. Existing consumers and services using the manual try/finally pattern SHALL continue to work during the transition period.

#### Scenario: Old consumer pattern still works
- **WHEN** a consumer still uses the manual `SetSystemUserContext`/`ClearSystemUserContext` pattern
- **AND** the `SystemUserContextConsumeFilter` is registered in the pipeline
- **THEN** the filter sets context first, then the consumer overwrites it with its manual call
- **AND** cleanup happens both in the consumer's `finally` and the filter's `finally`
- **AND** the system operates correctly (no errors from double-clear)
