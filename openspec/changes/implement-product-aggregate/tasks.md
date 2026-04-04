## 1. Breaking Changes and Foundations

- [ ] 1.1 Rename `VariantAddedEvent` to `VariantCreatedEvent` — update class name, `[EventVersion]` attribute, and all references in `ProductAggregate.AddVariant()` and its `Apply()` handler
- [ ] 1.2 Change `Variant.Price` and `Variant.DiscountPrice` from `double` to `decimal` — update the entity, `VariantCreatedEvent` (formerly `VariantAddedEvent`), and the `AddVariant()` behavior method signature
- [ ] 1.3 Update `ProductStateMachine` — replace `Ignore(ProductAction.Update)` in Draft with `PermitReentry(ProductAction.Update)`, add `PermitReentry(ProductAction.Update)` to Published and Unpublished states

## 2. Product Lifecycle — Publish

- [ ] 2.1 Create `Products/Publish/` folder with `PublishProductCommand` and `PublishProductCommandHandler`
- [ ] 2.2 Create `ProductPublishedEvent` domain event with `ProductId`, `PublishedAtUtc`, `PublishedByUserId` and `[EventVersion]` attribute
- [ ] 2.3 Create `ProductCanPublishSpecification` — validate `State.CanFire(Publish)`, ≥1 variant exists, ≥1 variant with Price > 0, Name not empty, Slug not empty, CategoryId not `Guid.Empty`
- [ ] 2.4 Add `Publish()` behavior method on `ProductAggregate` that validates specification and raises `ProductPublishedEvent`
- [ ] 2.5 Add `Apply(ProductPublishedEvent)` handler on `ProductAggregate` that fires `ProductAction.Publish` on the state machine and sets `LastModified` fields
- [ ] 2.6 Create `PublishProductEndpointHandler` (Minimal API POST endpoint)

## 3. Product Lifecycle — Unpublish

- [ ] 3.1 Create `Products/Unpublish/` folder with `UnpublishProductCommand` and `UnpublishProductCommandHandler`
- [ ] 3.2 Create `ProductUnpublishedEvent` domain event with `ProductId`, `UnpublishedAtUtc`, `UnpublishedByUserId`
- [ ] 3.3 Create `ProductCanUnpublishSpecification` — validate `State.CanFire(Unpublish)`
- [ ] 3.4 Add `Unpublish()` behavior method on `ProductAggregate` that validates specification and raises `ProductUnpublishedEvent`
- [ ] 3.5 Add `Apply(ProductUnpublishedEvent)` handler that fires `ProductAction.Unpublish`
- [ ] 3.6 Create `UnpublishProductEndpointHandler`

## 4. Product Lifecycle — Delete

- [ ] 4.1 Create `Products/Delete/` folder with `DeleteProductCommand` and `DeleteProductCommandHandler`
- [ ] 4.2 Create `ProductDeletedEvent` domain event with `ProductId`, `DeletedAtUtc`, `DeletedByUserId`
- [ ] 4.3 Create `ProductCanDeleteSpecification` — validate `State.CanFire(Delete)`
- [ ] 4.4 Add `Delete()` behavior method on `ProductAggregate` that validates specification and raises `ProductDeletedEvent`
- [ ] 4.5 Add `Apply(ProductDeletedEvent)` handler that fires `ProductAction.Delete`
- [ ] 4.6 Create `DeleteProductEndpointHandler`

## 5. Variation Dimension — Add

- [ ] 5.1 Create `Products/AddVariationDimension/` folder with `AddVariationDimensionCommand` and `AddVariationDimensionCommandHandler`
- [ ] 5.2 Create `VariationDimensionAddedEvent` domain event with `ProductId`, `Name`, `DisplayName`, `Values`, `DisplayStyle`
- [ ] 5.3 Create `CanAddVariationDimensionSpecification` — validate product not Deleted, name unique (case-insensitive), ≥1 value, values unique (case-insensitive), no non-default variants exist
- [ ] 5.4 Add `AddVariationDimension()` behavior method on `ProductAggregate`
- [ ] 5.5 Add `Apply(VariationDimensionAddedEvent)` handler that adds new `VariationDimension` to the list
- [ ] 5.6 Create `AddVariationDimensionEndpointHandler`

## 6. Variation Dimension — Update

- [ ] 6.1 Create `Products/UpdateVariationDimension/` folder with `UpdateVariationDimensionCommand` and `UpdateVariationDimensionCommandHandler`
- [ ] 6.2 Create `VariationDimensionUpdatedEvent` domain event with `ProductId`, `Name`, `DisplayName`, `DisplayStyle`
- [ ] 6.3 Create `CanUpdateVariationDimensionSpecification` — validate product not Deleted, dimension with name exists
- [ ] 6.4 Add `UpdateVariationDimension()` behavior method on `ProductAggregate`
- [ ] 6.5 Add `Apply(VariationDimensionUpdatedEvent)` handler that updates matching dimension's `DisplayName` and `DisplayStyle`
- [ ] 6.6 Create `UpdateVariationDimensionEndpointHandler`

## 7. Variation Dimension — Change Values

- [ ] 7.1 Create `Products/ChangeVariationDimensionValues/` folder with `ChangeVariationDimensionValuesCommand` and `ChangeVariationDimensionValuesCommandHandler`
- [ ] 7.2 Create `VariationDimensionValuesChangedEvent` domain event with `ProductId`, `DimensionName`, `Values`
- [ ] 7.3 Create `CanChangeVariationDimensionValuesSpecification` — validate product not Deleted, dimension exists, ≥1 new value, values unique, no removed value referenced by a non-deleted variant
- [ ] 7.4 Add `ChangeVariationDimensionValues()` behavior method on `ProductAggregate`
- [ ] 7.5 Add `Apply(VariationDimensionValuesChangedEvent)` handler that replaces matching dimension's `Values` array
- [ ] 7.6 Create `ChangeVariationDimensionValuesEndpointHandler`

## 8. Variant — Enhance AddVariant Specification

- [ ] 8.1 Enhance `ProductCanAddVariantSpecification` — add value membership check (each dimension value's `Value` ∈ `VariationDimension.Values`)
- [ ] 8.2 Enhance `ProductCanAddVariantSpecification` — add duplicate combination check (no existing non-deleted variant with same dimension value set)

## 9. Variant — Update

- [ ] 9.1 Create `Products/UpdateVariant/` folder with `UpdateVariantCommand` and `UpdateVariantCommandHandler`
- [ ] 9.2 Create `VariantUpdatedEvent` domain event with `ProductId`, `VariantId`, `Name`, `Sku`, `UpdatedAtUtc`, `UpdatedByUserId`
- [ ] 9.3 Create `CanUpdateVariantSpecification` — validate product not Deleted, variant exists, variant not Deleted
- [ ] 9.4 Add `UpdateVariant()` behavior method on `ProductAggregate`
- [ ] 9.5 Add `Apply(VariantUpdatedEvent)` handler that updates matching variant's `Name` and `Sku`
- [ ] 9.6 Create `UpdateVariantEndpointHandler`

## 10. Variant — Change Price

- [ ] 10.1 Create `Products/ChangeVariantPrice/` folder with `ChangeVariantPriceCommand` and `ChangeVariantPriceCommandHandler`
- [ ] 10.2 Create `VariantPriceChangedEvent` domain event with `ProductId`, `VariantId`, `OldPrice`, `NewPrice`, `OldDiscountPrice`, `NewDiscountPrice`, `ChangedAtUtc`, `ChangedByUserId`
- [ ] 10.3 Create `CanChangeVariantPriceSpecification` — validate product not Deleted, variant exists and not Deleted, Price > 0, DiscountPrice ≥ 0, DiscountPrice ≤ Price
- [ ] 10.4 Add `ChangeVariantPrice()` behavior method on `ProductAggregate`
- [ ] 10.5 Add `Apply(VariantPriceChangedEvent)` handler that updates matching variant's `Price` and `DiscountPrice`
- [ ] 10.6 Create `ChangeVariantPriceEndpointHandler`

## 11. Variant — Publish

- [ ] 11.1 Create `Products/PublishVariant/` folder with `PublishVariantCommand` and `PublishVariantCommandHandler`
- [ ] 11.2 Create `VariantPublishedEvent` domain event with `ProductId`, `VariantId`, `PublishedAtUtc`, `PublishedByUserId`
- [ ] 11.3 Create `CanPublishVariantSpecification` — validate product not Deleted, variant exists and not Deleted, variant is Unpublished, SKU not empty, Price > 0, dimension values complete (for non-default variants)
- [ ] 11.4 Add `PublishVariant()` behavior method on `ProductAggregate`
- [ ] 11.5 Add `Apply(VariantPublishedEvent)` handler that sets variant `State` to `Published`
- [ ] 11.6 Create `PublishVariantEndpointHandler`

## 12. Variant — Unpublish

- [ ] 12.1 Create `Products/UnpublishVariant/` folder with `UnpublishVariantCommand` and `UnpublishVariantCommandHandler`
- [ ] 12.2 Create `VariantUnpublishedEvent` domain event with `ProductId`, `VariantId`, `UnpublishedAtUtc`, `UnpublishedByUserId`
- [ ] 12.3 Create `CanUnpublishVariantSpecification` — validate product not Deleted, variant exists and not Deleted, variant is Published, NOT last published variant if Product is Published
- [ ] 12.4 Add `UnpublishVariant()` behavior method on `ProductAggregate`
- [ ] 12.5 Add `Apply(VariantUnpublishedEvent)` handler that sets variant `State` to `Unpublished`
- [ ] 12.6 Create `UnpublishVariantEndpointHandler`

## 13. Integration Events

- [ ] 13.1 Add `ProductUpdated` integration event to `ProductIntegrationEvents.cs` — `ProductId`, `Name`, `Description`, `CategoryId`, `Tags`, `Slug`, `Images`, `Groups`
- [ ] 13.2 Add `ProductPublished`, `ProductUnpublished`, `ProductDeleted` integration events to `ProductIntegrationEvents.cs` — each with `ProductId`
- [ ] 13.3 Add `VariationDimensionAdded`, `VariationDimensionUpdated`, `VariationDimensionValuesChanged` integration events to `ProductIntegrationEvents.cs`
- [ ] 13.4 Add `VariantCreated` integration event — `ProductId`, `VariantId`, `Name`, `Sku`, `Price` (decimal), `DiscountPrice` (decimal), `IsDefault`, `VariantDimensionValues`
- [ ] 13.5 Add `VariantUpdated`, `VariantPriceChanged`, `VariantPublished`, `VariantUnpublished` integration events to `ProductIntegrationEvents.cs`
- [ ] 13.6 Create domain event subscribers for all 13 events that publish corresponding integration events via `IEventBus`

## 14. Read Model — Product Document and Persistence

- [ ] 14.1 Create `Product` read model entity in `Models/` — implement `IEntityBase<string>`, `IScoped`, add all properties with JADNC `[Resource]` and `[Attr]` annotations
- [ ] 14.2 Create `ProductVariationDimension`, `ProductVariant`, `ProductVariantDimensionValue` embedded model classes in `Models/`
- [ ] 14.3 Create `IProductReadRepository` interface in `Models/` extending `IRepositoryBase<Product, string>`
- [ ] 14.4 Create `ProductReadRepository` in `Persistence/` extending `RepositoryBase<CatalogReadDbContext, Product, string>`
- [ ] 14.5 Create `ProductEntityConfiguration` in `Persistence/EntityConfigurations/` — map to `Product` collection, configure embedded types, configure MongoDB indexes (tenant+state, tenant+category, tenant+slug, variant SKU, variant price)
- [ ] 14.6 Add `DbSet<Product>` to `CatalogReadDbContext`
- [ ] 14.7 Register `IProductReadRepository` as scoped service in DI bootstrapping

## 15. Read Model — Consumers and Projection Handlers

- [ ] 15.1 Create `ProductCreatedConsumer` and `CreateProductProjectionCommandHandler` — insert new Product document with state "Draft"
- [ ] 15.2 Create `ProductUpdatedConsumer` and `UpdateProductProjectionCommandHandler` — update product metadata fields
- [ ] 15.3 Create `ProductPublishedConsumer` and `PublishProductProjectionCommandHandler` — set state to "Published"
- [ ] 15.4 Create `ProductUnpublishedConsumer` and `UnpublishProductProjectionCommandHandler` — set state to "Unpublished"
- [ ] 15.5 Create `ProductDeletedConsumer` and `DeleteProductProjectionCommandHandler` — set state to "Deleted"
- [ ] 15.6 Create `VariationDimensionAddedConsumer` and `AddVariationDimensionProjectionCommandHandler` — push dimension into VariationDimensions array
- [ ] 15.7 Create `VariationDimensionUpdatedConsumer` and `UpdateVariationDimensionProjectionCommandHandler` — update matching dimension's DisplayName and DisplayStyle
- [ ] 15.8 Create `VariationDimensionValuesChangedConsumer` and `ChangeVariationDimensionValuesProjectionCommandHandler` — replace matching dimension's Values array
- [ ] 15.9 Create `VariantCreatedConsumer` and `CreateVariantProjectionCommandHandler` — push variant into Variants array with state "Unpublished"
- [ ] 15.10 Create `VariantUpdatedConsumer` and `UpdateVariantProjectionCommandHandler` — update matching variant's Name and Sku
- [ ] 15.11 Create `VariantPriceChangedConsumer` and `ChangeVariantPriceProjectionCommandHandler` — update matching variant's Price and DiscountPrice
- [ ] 15.12 Create `VariantPublishedConsumer` and `PublishVariantProjectionCommandHandler` — set matching variant's State to "Published"
- [ ] 15.13 Create `VariantUnpublishedConsumer` and `UnpublishVariantProjectionCommandHandler` — set matching variant's State to "Unpublished"

## 16. Read Model — Controller and Registration

- [ ] 16.1 Create `ProductsController` in `Controllers/` — thin JADNC wrapper with `[RequireFeature]` and `[RequireOneOfPermissions]` attributes, following `CategoriesController` pattern
- [ ] 16.2 Register all 13 Product consumers with MassTransit in `AddMassTransitRabbitMQ()` bootstrapping method

## 17. Unit Tests — Write Model

- [ ] 17.1 Write unit tests for `ProductStateMachine` — verify `PermitReentry(Update)` in Draft/Published/Unpublished, `CanFire` returns for all states and actions
- [ ] 17.2 Write unit tests for `ProductCanPublishSpecification` — all 6 validation rules with positive and negative cases
- [ ] 17.3 Write unit tests for `ProductCanUnpublishSpecification` and `ProductCanDeleteSpecification`
- [ ] 17.4 Write unit tests for `CanAddVariationDimensionSpecification` — name uniqueness, values validation, non-default variant guard
- [ ] 17.5 Write unit tests for `CanUpdateVariationDimensionSpecification` and `CanChangeVariationDimensionValuesSpecification` — including variant reference safety
- [ ] 17.6 Write unit tests for enhanced `ProductCanAddVariantSpecification` — value membership and combination uniqueness
- [ ] 17.7 Write unit tests for `CanUpdateVariantSpecification`, `CanChangeVariantPriceSpecification`, `CanPublishVariantSpecification`, `CanUnpublishVariantSpecification`
- [ ] 17.8 Write unit tests for all `ProductAggregate` behavior methods — verify correct events raised and Apply handlers mutate state correctly
