## 1. Project Dependencies

- [x] 1.1 Add `Testcontainers.MongoDb` package to `EShop.Catalog.Tests.csproj`
- [x] 1.2 Add project reference to `EShop.Catalog.ReadModels.MongoDb` in `EShop.Catalog.Tests.csproj`
- [ ] 1.3 Verify the test project builds successfully with the new dependencies

## 2. MongoDB Test Database Helper

- [x] 2.1 Create `MongoDbTestDatabase` class in `Catalog/tests/EShop.Catalog.Tests/Setup/` with container reference, per-scenario database name (`catalog_test_{Guid:N}`), connection string accessor, database name accessor, and `DropAsync()` method

## 3. Test Container Lifecycle (ScenarioHooks)

- [x] 3.1 Add static `MongoDbContainer` field to `ScenarioHooks` and start it in `[BeforeTestRun]` with dynamic port binding
- [x] 3.2 In `[BeforeScenario]`, create a `MongoDbTestDatabase` instance with unique database name and register it in the object container
- [x] 3.3 In `[AfterScenario]`, call `MongoDbTestDatabase.DropAsync()` to clean up the per-scenario database
- [x] 3.4 In `[AfterTestRun]`, stop and dispose the `MongoDbContainer`

## 4. DI Registration (ServiceCollectionExtensions)

- [x] 4.1 Add `AddCatalogReadModelTestServices` method that registers `IMongoDbSettings` (pointing to test container), `IMongoDatabase` singleton, and `IMongoRepositoryBase<>` scoped
- [x] 4.2 Register `IMediator` with command handlers from both `Application` and `ReadModels.MongoDb` assemblies
- [x] 4.3 Update `AddMassTransitInMemory` to register consumers from both `Application.AssemblyReference.Assembly` and `ReadModels.MongoDb.AssemblyReference.Assembly`

## 5. Test Startup Wiring

- [ ] 5.1 Update `TestStartup.ConfigureServices` to call `AddCatalogReadModelTestServices` and pass `MongoDbTestDatabase` into the startup
- [ ] 5.2 Update `ApiContext` constructor to accept and pass `MongoDbTestDatabase` to `TestStartup`

## 6. BDD Feature: CreateCategory End-to-End

- [ ] 6.1 Update `CreateCategory.feature` — implement the `Then` step to verify projected category data (Name, Reference, Slug, ParentId) from MongoDB
- [ ] 6.2 Add scenario for creating a category with a parent and verifying `ParentId` in the MongoDB projection
- [ ] 6.3 Implement `StepContext` method to query MongoDB via `IMongoRepositoryBase<Category>` by Reference
- [ ] 6.4 Implement `Then` step definition in `Steps.cs` that compares DataTable expectations against the MongoDB document
- [ ] 6.5 Run CreateCategory tests and verify they pass end-to-end

## 7. BDD Feature: UpdateCategory End-to-End

- [ ] 7.1 Create `UpdateCategory.feature` with scenario: create a category, update its name, verify MongoDB projection reflects the update
- [ ] 7.2 Create `Steps.cs` and `StepContext.cs` for UpdateCategory with `When` step calling the update API and `Then` step querying MongoDB
- [ ] 7.3 Run UpdateCategory tests and verify they pass end-to-end

## 8. BDD Feature: Idempotent Consumer Verification

- [ ] 8.1 Create a feature scenario that publishes the same `CategoryCreated` integration event twice and verifies only one MongoDB document exists
- [ ] 8.2 Implement step that queries `IMongoRepositoryBase<InboxMessage>` to verify the inbox entry exists
- [ ] 8.3 Run idempotency tests and verify they pass

## 9. Final Validation

- [ ] 9.1 Run full `dotnet test Catalog/tests/EShop.Catalog.Tests` and verify all scenarios pass
- [ ] 9.2 Verify no test interference — multiple scenarios running sequentially use isolated databases
