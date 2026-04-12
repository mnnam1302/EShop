## Why

The Product Aggregate currently supports only 3 of 13 domain events (ProductCreated, ProductUpdated, VariantAdded). The full product lifecycle — publishing, unpublishing, deleting, variation dimension management, and variant operations — is unimplemented, blocking the Catalog from serving a functional e-commerce storefront. Phase 1 (Event Storming & design) is complete and approved; Phase 2 implementation must begin now.

## What Changes

- **Rename** `VariantAddedEvent` → `VariantCreatedEvent` for naming consistency across all 13 events
- **Change** `double` → `decimal` for `Price` and `DiscountPrice` on `Variant` (financial accuracy) — **BREAKING** (event schema change)
- **Update** `ProductStateMachine` to allow `Update` as a self-loop in Draft, Published, and Unpublished states
- **Add** 10 new domain events: `ProductPublished`, `ProductUnpublished`, `ProductDeleted`, `VariationDimensionAdded`, `VariationDimensionUpdated`, `VariationDimensionValuesChanged`, `VariantUpdated`, `VariantPriceChanged`, `VariantPublished`, `VariantUnpublished`
- **Add** corresponding aggregate behavior methods (`Publish()`, `Unpublish()`, `Delete()`, `AddVariationDimension()`, `UpdateVariationDimension()`, `ChangeVariationDimensionValues()`, `UpdateVariant()`, `ChangeVariantPrice()`, `PublishVariant()`, `UnpublishVariant()`) with `Apply()` handlers
- **Create** 12 new Specification classes enforcing domain invariants (publish requires ≥1 variant with price, dimension name uniqueness, variant combination uniqueness, unpublish-last-variant guard, etc.)
- **Create** 10 new vertical slice folders with Commands, Handlers, and Endpoint handlers
- **Add** 12 new integration event classes to `ProductIntegrationEvents` in Shared
- **Create** Product MongoDB read model document with nested VariationDimension and Variant sub-documents
- **Create** 13 MassTransit consumers and projection command handlers for the read model
- **Add** MongoDB indexes for tenant+state, tenant+category, tenant+slug, variant SKU, and variant price

## Capabilities

### New Capabilities
- `product-lifecycle`: Product state transitions — Publish, Unpublish, Delete with specifications and state machine enforcement
- `variation-dimension-management`: Add, update, and change values of variation dimensions on a Product, with invariant checks (name uniqueness, variant-reference safety)
- `variant-operations`: Create, update, price change, publish, and unpublish variants with specifications (combination uniqueness, price validation, last-published-variant guard)
- `product-read-projection`: MongoDB read model document for Product with nested dimensions/variants, 13 consumers, projection handlers, and indexes

### Modified Capabilities
- `catalog-read-persistence`: Adding Product document model, DbSet, and entity configuration alongside existing Category infrastructure
- `catalog-read-project-structure`: Adding Product consumers, handlers, and model files into the existing folder structure

## Impact

- **Code**: `EShop.Catalog.Application` (aggregate, events, specifications, commands, handlers, endpoints), `EShop.Catalog.ReadModels.MongoDb` (model, consumers, handlers, persistence), `Shared/.../ProductIntegrationEvents.cs`
- **APIs**: 10 new write endpoints (Minimal API), existing read endpoints gain Product data
- **Event Store**: 10 new event types persisted to PostgreSQL; 1 renamed event (`VariantAdded` → `VariantCreated`); 1 schema change (`double` → `decimal` for price fields)
- **Message Bus**: 12 new integration event types published to RabbitMQ
- **MongoDB**: New `products` collection with 5 indexes
- **Backward Compatibility**: Existing `ProductCreated` and `ProductUpdated` events/integration events unchanged. `VariantAdded` rename and price type change are breaking for any pre-existing event streams (acceptable — feature branch, no production data)
