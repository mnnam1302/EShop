## ADDED Requirements

### Requirement: MongoDB Testcontainer lifecycle management
The test infrastructure SHALL start a single MongoDB container in `[BeforeTestRun]` and stop/dispose it in `[AfterTestRun]`, matching the existing PostgreSQL Testcontainer pattern.

#### Scenario: MongoDB container starts once per test run
- **WHEN** the test run begins
- **THEN** a MongoDB Testcontainer SHALL be started with a dynamically assigned port

#### Scenario: MongoDB container is shared across scenarios
- **WHEN** multiple scenarios execute within a single test run
- **THEN** all scenarios SHALL use the same MongoDB container instance

#### Scenario: MongoDB container is disposed after test run
- **WHEN** the test run completes
- **THEN** the MongoDB container SHALL be stopped and disposed

### Requirement: Per-scenario MongoDB database isolation
The test infrastructure SHALL create a unique MongoDB database for each scenario and drop it after the scenario completes, preventing data leakage between scenarios.

#### Scenario: Unique database per scenario
- **WHEN** a new scenario begins
- **THEN** a MongoDB database with a unique name (format: `catalog_test_{guid}`) SHALL be created

#### Scenario: Database cleanup after scenario
- **WHEN** a scenario completes (pass or fail)
- **THEN** the per-scenario MongoDB database SHALL be dropped

### Requirement: MongoDbTestDatabase helper class
The test infrastructure SHALL provide a `MongoDbTestDatabase` helper class analogous to `PostgreSqlTestDatabase` that encapsulates container reference, per-scenario database naming, connection string access, and cleanup.

#### Scenario: Helper exposes connection string and database name
- **WHEN** a test step needs to configure MongoDB services
- **THEN** `MongoDbTestDatabase` SHALL provide the container's connection string and the per-scenario database name

#### Scenario: Helper supports drop for cleanup
- **WHEN** a scenario ends and cleanup is needed
- **THEN** `MongoDbTestDatabase.DropAsync()` SHALL drop the per-scenario database from the shared container

### Requirement: Composite test host with write and read side DI
The test `ServiceCollectionExtensions` SHALL register services from both `EShop.Catalog.Application` (write side) and `EShop.Catalog.ReadModels.MongoDb` (read side) into a single DI container within the test host.

#### Scenario: MongoDB persistence services registered
- **WHEN** the test host is configured
- **THEN** `IMongoRepositoryBase<T>`, `IMongoDbSettings`, and `IMongoDatabase` SHALL be registered with the test container's connection details

#### Scenario: ReadModels mediator handlers registered
- **WHEN** the test host is configured
- **THEN** command handlers from the `ReadModels.MongoDb` assembly (e.g., `CreateCategoryProjectionCommandHandler`) SHALL be resolvable via `IMediator`

### Requirement: MassTransit consumers from both assemblies
The in-memory MassTransit configuration SHALL register consumers from both the `Application` assembly (write-side consumers like `OrganizationCreatedConsumer`) and the `ReadModels.MongoDb` assembly (read-side consumers like `CategoryCreatedConsumer`, `CategoryUpdatedConsumer`).

#### Scenario: Write-side and read-side consumers both active
- **WHEN** an integration event is published via `IEventBus`
- **THEN** consumers from both assemblies SHALL be invoked by the in-memory MassTransit bus

#### Scenario: TestConsumeObserver tracks all consumers
- **WHEN** a read-side consumer processes an integration event
- **THEN** `TestConsumeObserver.WaitForQuietAsync()` SHALL wait for all consumers (including read-side) to settle before returning

### Requirement: Test project dependencies
The `EShop.Catalog.Tests.csproj` SHALL reference `EShop.Catalog.ReadModels.MongoDb` and include the `Testcontainers.MongoDb` package.

#### Scenario: Project builds with MongoDB dependencies
- **WHEN** the test project is compiled
- **THEN** it SHALL successfully resolve types from `EShop.Catalog.ReadModels.MongoDb` and `Testcontainers.MongoDb`
