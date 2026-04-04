## Context

The Product Aggregate in the Catalog microservice currently has 3 of 13 planned domain events (`ProductCreated`, `ProductUpdated`, `VariantAdded`) and 1 of 13 integration events (`ProductCreated`). The full lifecycle — publishing, unpublishing, deleting, variation dimension management, and variant sub-operations — is unimplemented. Phase 1 Event Storming and design (documented in `Products/README.md`) is complete and approved.

The existing codebase establishes clear patterns via the Category aggregate (write-side) and its read model consumers (read-side), which serve as the reference implementation. The Product aggregate must follow these patterns while introducing additional complexity: child entities (Variant), value objects (VariationDimension), and a nested MongoDB document structure.

### Current State

- **Write model**: `ProductAggregate` with `Create()`, `Update()`, `AddVariant()` behaviors using the Stateless library for `ProductStateMachine`
- **Read model**: Zero Product consumers or projections exist; Category consumers demonstrate the `IdempotentConsumer<T>` → `IMediator` → projection handler pattern
- **Integration events**: Only `ProductCreated` in `ProductIntegrationEvents.cs`
- **Specifications**: `ProductCanUpdateSpecification` (state check), `ProductCanAddVariantSpecification` (default variant rules, dimension count validation — missing value membership and duplicate combination checks)
- **Data types**: `Variant.Price` and `Variant.DiscountPrice` are `double` (incorrect for financial data)
- **State machine**: `Draft` ignores `Update` (should be a self-loop); `Published` and `Unpublished` have no `Update` transition

## Goals / Non-Goals

**Goals:**

- Implement the full Product lifecycle (Publish, Unpublish, Delete) with state machine enforcement
- Implement variation dimension management (Add, Update, ChangeValues) with invariant checks
- Implement variant operations (Create, Update, PriceChange, Publish, Unpublish) with specifications
- Create 12 new integration events and 13 MassTransit consumers projecting to a nested MongoDB document
- Fix `double` → `decimal` for price fields
- Rename `VariantAdded` → `VariantCreated` for naming consistency
- Follow existing Vertical Slice and Category reference patterns exactly

**Non-Goals:**

- Variant deletion (deferred to a future change — delete semantics need policy design for order references)
- Full-text search or Elasticsearch integration
- Product image upload/storage (images remain URL strings)
- Inventory or stock management (separate bounded context)
- Pricing rules, tiered pricing, or currency handling
- API versioning for the new Product endpoints (greenfield, no existing consumers)
- Performance optimization or caching for read queries (premature at this stage)

## Decisions

### 1. Breaking changes are acceptable — no production data exists

**Decision**: Rename `VariantAddedEvent` → `VariantCreatedEvent` and change `double` → `decimal` for price fields without `IEventUpgrader`.

**Rationale**: This is a feature branch with no production event streams. Both changes improve correctness — `decimal` avoids floating-point rounding in financial calculations, and consistent `*Created` naming aligns with the event naming convention (`ProductCreated`, `VariantCreated`).

**Alternative considered**: Implement `IEventUpgrader` for backward compatibility. Rejected — unnecessary complexity for zero existing data.

### 2. Update as state machine self-loop, not Ignore

**Decision**: Change `ProductStateMachine` so `Update` is a `PermitReentry` in Draft, Published, and Unpublished states (instead of `Ignore` in Draft and absent elsewhere).

**Rationale**: `Ignore` silently swallows the transition, which is correct when you want no-op. But `Update` should actively succeed (not silently no-op) so `ProductCanUpdateSpecification` can use `State.CanFire(Update)` as a guard. `PermitReentry` fires the transition and returns to the same state, making the specification check meaningful.

**Alternative considered**: Keep `Ignore` for Draft and add `PermitReentry` only for Published/Unpublished. Rejected — inconsistent behavior across states.

### 3. VariantState remains an enum, not a Stateless machine

**Decision**: Keep `VariantState` as an enum (`Published`, `Unpublished`, `Deleted`). Validate transitions in Specifications rather than a state machine.

**Rationale**: Variants are child entities (many per aggregate). Stateless machines carry per-instance overhead and require constructor injection of state accessor/mutator. Specifications already enforce every variant operation; a state machine would duplicate these checks without adding value. The variant lifecycle is simple (3 states, 4 transitions) — an enum is sufficient.

**Alternative considered**: `VariantStateMachine` analogous to `ProductStateMachine`. Rejected — over-engineering for a child entity with simple transitions.

### 4. Separate VariantPriceChanged from VariantUpdated

**Decision**: Price changes emit `VariantPriceChanged` (with old/new Price and DiscountPrice). Name/SKU/metadata changes emit `VariantUpdated`.

**Rationale**: Price changes have distinct business significance — they trigger different notifications, audit entries, and (future) pricing analytics subscribers. Separating them provides a clean event contract for downstream consumers. The `ChangeVariantPrice` command takes only price fields; `UpdateVariant` takes metadata fields. This follows the CQS principle at the event level.

**Alternative considered**: Single `VariantUpdated` event with all fields. Rejected — conflates metadata edits with financially significant price changes.

### 5. Dimension value removal blocked by variant references

**Decision**: `CanChangeVariationDimensionValuesSpecification` rejects removal of any value that is referenced by an existing non-deleted variant's `VariantDimensionValues`.

**Rationale**: Removing a dimension value that a variant depends on would leave orphaned references. In a SaaS multi-tenant system, data integrity errors are costly — merchants cannot easily recover from corrupted product data. The safest approach is to block removal and require the merchant to update or delete affected variants first.

**Alternative considered**: Cascade-delete or cascade-unpublish affected variants. Rejected — implicit data destruction in a SaaS context is dangerous and surprising.

### 6. Cannot add new dimensions when non-default variants exist

**Decision**: `CanAddVariationDimensionSpecification` rejects adding a new dimension if any non-default variant exists on the product.

**Rationale**: Adding a dimension changes the cardinality of the variant matrix. Existing variants would lack values for the new dimension, violating the invariant that every non-default variant must have exactly one value per dimension. Rather than silently invalidating existing variants, we block the operation and require the merchant to remove variants first.

**Alternative considered**: Auto-assign a default value to existing variants. Rejected — invents data the merchant didn't provide.

### 7. Nested MongoDB document, not flat collections

**Decision**: Product read model is a single MongoDB document with `variationDimensions[]` and `variants[]` as embedded arrays. No separate `variants` or `dimensions` collections.

**Rationale**: Products are always queried as a whole — storefront pages need dimensions and variants together. Embedding avoids joins (which MongoDB lacks natively) and enables atomic updates. The variant array is bounded (typical products have 10-50 variants, max ~500), well within MongoDB's 16MB document limit.

**Alternative considered**: Separate `variants` collection with `productId` reference. Rejected — requires two queries per product page and complicates consistency.

### 8. One integration event per domain event (1:1 mapping)

**Decision**: Each of the 13 domain events maps to exactly one integration event in `ProductIntegrationEvents.cs`, published via `IEventBus` → RabbitMQ.

**Rationale**: The read model needs every state change to maintain an accurate projection. Combining or filtering events at the publisher would create coupling between the write side's decisions and the read side's needs. 1:1 mapping keeps the contract simple — consumers can choose which events to subscribe to.

**Alternative considered**: Coarse-grained integration events (e.g., `ProductChanged` with a change type). Rejected — forces consumers to parse change types and handle a god-event.

### 9. Consumer → Mediator → Projection Handler (Category pattern)

**Decision**: Each MassTransit consumer extends `IdempotentConsumer<T>`, maps the integration event to a projection command, and dispatches via `IMediator.SendAsync()`. The projection handler performs the MongoDB write.

**Rationale**: This is the established pattern from `CategoryCreatedConsumer`. Separation of consumer (transport concern) and handler (persistence concern) keeps each class focused. Idempotent consumers use the inbox (`InboxMessage` table) to prevent duplicate processing.

**Alternative considered**: Consumers write to MongoDB directly. Rejected — violates the existing pattern and mixes transport with persistence.

### 10. Vertical Slice folder structure per operation

**Decision**: Each aggregate operation gets its own folder under `Products/` containing Command, CommandHandler, domain Event, Specification, and EndpointHandler. Folder names follow the existing convention (e.g., `Publish/`, `AddVariant/`, `UpdateVariant/`).

**Rationale**: Vertical Slice Architecture keeps all code for one operation co-located, making it easy to understand, test, and modify in isolation. This matches the existing `Create/` and `Update/` folder structure in the Products domain and the broader project convention.

**Alternative considered**: Group by technical concern (all commands in one folder, all events in another). Rejected — contradicts the existing codebase convention and scatters related code.

## Risks / Trade-offs

**[Risk] Event schema breaking changes block future replays** → Mitigated by the fact that no production event streams exist. If pre-existing test data needs to be preserved, a one-time migration script can be written — but this is not expected.

**[Risk] Nested MongoDB document grows large for products with many variants** → Mitigated by the natural business constraint (most products have < 100 variants). If extreme cases emerge, a future change can introduce pagination at the API level or document splitting. MongoDB's 16MB limit accommodates ~50,000 embedded variants.

**[Risk] 13 consumers increase the RabbitMQ queue surface area** → Mitigated by MassTransit's automatic queue management and the idempotent consumer pattern. Each consumer is stateless and independently scalable. Queue fanout is a standard CQRS trade-off.

**[Risk] Dimension value removal check (Decision 5) may feel restrictive to merchants** → Mitigated by clear error messages explaining which variants reference the value. A future UX improvement could offer a "remove value and unpublish affected variants" batch operation.

**[Risk] Blocking dimension addition (Decision 6) when variants exist adds friction** → This is a deliberate trade-off favoring data integrity over convenience. The merchant workflow is: define dimensions → create variants. Re-dimensioning a product is an exceptional operation, not a daily one.

**[Trade-off] VariantState as enum means transition logic is spread across specifications** → Accepted because the variant lifecycle is simple (3 states, 4 transitions). If variant lifecycle grows more complex (e.g., "PendingApproval", "Suspended"), this decision should be revisited in favor of a state machine.

**[Trade-off] No variant deletion in this change** → Accepted to limit scope. Variant deletion has downstream implications (order history, analytics references) that require separate design. Variants can be unpublished to effectively "hide" them.
