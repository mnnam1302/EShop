---
name: domain-driven-design
description: >
  Domain-Driven Design tactical patterns for .NET applications. Covers aggregates,
  aggregate roots, value objects, domain events, domain services, strongly-typed IDs,
  and repository patterns for aggregate persistence.
  Load this skill when implementing DDD, working with aggregates, value objects,
  domain events, bounded contexts, or when the architecture-advisor recommends
  DDD + Clean Architecture. Pair with the clean-architecture skill.
---

# Domain-Driven Design (DDD)
DDD mostly focuses on the Domain & Application Layers and ignores the API/Presentation and Infrastructure. 

## Domain Layer Building Blocks
1. **Entity**: An Entity is an object with its own properties (state, data) and methods that implements the business logic that is executed on these properties. An entity is represented by its unique identifier (Id). Two entity object with different Ids are considered as different entities.
2. **Value Object**:  A Value Object is another kind of domain object that is identified by its properties rather than a unique Id. That means two Value Objects with same properties are considered as the same object. Value objects are generally implemented as immutable and mostly are much simpler than the Entities.
3. **Domain Event**: A Domain Event is a way of informing other services in a loosely coupled manner, when a domain specific event happens in the domain (OrderPlaced, PaymentReceived), raise a domain event. Side effects (send email, update read model, notify another aggregate) subscribe to these events. The aggregate stays focused on its own rules.
4. **Aggregate & Aggregate Root**: An Aggregate is a cluster of objects (entities and value objects) bound together by an Aggregate Root object. The Aggregate Root is a specific type of an entity with some additional responsibilities.
5. **Repository (interface)**: A Repository is a collection-like interface that is used by the Domain and Application Layers to access to the data persistence system (the database). It hides the complexity of the DBMS from the business code. Domain Layer contains the interfaces of the repositories.
6. **Domain Service**: A Domain Service is a stateless service that implements core business rules of the domain. It is useful to implement domain logic that depends on multiple aggregate (entity) type or some external services.
7. **Specification**: A Specification is used to define named, reusable and combinable filters for entities and other business objects.

## Patterns

### Aggregate / Aggregate Root Principles

#### Business Rules
- Entities are responsible to implement the business rules related to the properties of their own. The Aggregate Root Entities are also responsible for their sub-collection entities.
- An aggregate should maintain its self integrity and validity by 
implementing domain rules and constraints.

```csharp
// Domain/Aggregate/Orders/Order.cs
public sealed class Order : AggregateRoot
{
    private readonly List<OrderLine> _lines = [];

    private Order() { } // EF Core

    public OrderNumber Number { get; private set; } = null!;
    public CustomerId CustomerId { get; private set; }
    public Money Total { get; private set; } = Money.Zero("USD");
    public OrderStatus Status { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public static Order Place(CustomerId customerId, OrderNumber number, DateTimeOffset now)
    {
        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerId = customerId,
            Number = number,
            Status = OrderStatus.Placed,
            PlacedAt = now
        };

        order.RaiseDomainEvent(new OrderPlaced(order.Id, customerId, now));
        return order;
    }

    public Result AddLine(ProductId productId, int quantity, Money unitPrice)
    {
        if (Status is not OrderStatus.Placed)
            return Result.Failure("Cannot modify a confirmed or cancelled order");

        if (quantity <= 0)
            return Result.Failure("Quantity must be positive");

        var existing = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            _lines.Add(new OrderLine(productId, quantity, unitPrice));
        }

        RecalculateTotal();
        return Result.Success();
    }

    public Result Confirm()
    {
        if (Status is not OrderStatus.Placed)
            return Result.Failure("Only placed orders can be confirmed");

        if (_lines.Count == 0)
            return Result.Failure("Cannot confirm an order with no lines");

        Status = OrderStatus.Confirmed;
        RaiseDomainEvent(new OrderConfirmed(Id));
        return Result.Success();
    }

    private void RecalculateTotal()
    {
        Total = _lines.Aggregate(Money.Zero(Total.Currency), (sum, line) => sum + line.Subtotal);
    }
}
```

#### Single Unit
An aggregate is retrieved and saved as a single unit, with all the sub-collections and properties. For example, if you want to add a `OrderLine` to an `Order`, you need to:
1.  Get the `Order` from database with including all the sub-collections (`OrderLines`).
2.  Use methods on the `Order` class to add a new order line, 
like Order.AddLine(...);.
3. Save the `Order` (with all sub-collections) to the database as a single database operation (update).

#### Aggregate Boundary
- If a use case works with a single aggregate, reads and saves it as a single unit, all the changes made to the aggregate objects are saved together as an atomic operation and you don't need to an explicit database transaction.
- In real life, you may need to change more than one aggregate instances in a single use case and you need to use database transactions to ensure atomic update and data consistency. Because of that, we use an explicit database transaction for a use case (an application service boundary). See the skill `Unit Of Work`.

#### Aggregate / Aggregate Root Rules & Best Pratices
1. **Reference Other Aggregates Only by ID**: The first rule says an Aggregate should reference to other aggregates only by their Id. That means you can not add navigation properties to other aggregates.

2. **Keep Aggregates Small**: One good practice is to keep an aggregate simple and small. This is because an aggregate will be loaded and saved as a single unit and reading/writing a big object has performance problems. In pratical,
    - Most of the aggregate roots will not have sub-collections.
    -  A sub-collection should not have more than 100-150 items inside it at the most case. If you think a collection potentially can have more items, don't define the collection as a part of the aggregate and consider to extract another aggregate root for the entity inside the collection. 

### Value Objects as Records

Use C# records for immutable value objects with structural equality:

```csharp
public sealed record Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency) => new(0, currency);

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
          throw new InvalidOperationException($"Cannot add {left.Currency} and {right.Currency}");
        }

        return new Money(left.Amount + right.Amount, left.Currency);
    }
}

// Other value objects (EmailAddress, OrderNumber, etc.) follow the same pattern:
// sealed record, constructor validation, no public setters
```

### Domain Event Dispatching

Raise events in the aggregate, dispatch in SaveChangesAsync:

```csharp
// Shared/src/EShop.Shared.DomainTools/Aggregates/IAggregateRoot.cs
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Shared/src/EShop.Shared.Contracts/Abstractions/MessageBus/IEvent.cs
public interface IDomainEvent : IEvent
{
    ulong Version { get; set; }
}

// Domain/Orders/Events/OrderPlaced.cs
public sealed record OrderPlaced(Guid OrderId, CustomerId CustomerId, DateTimeOffset PlacedAt) : IDomainEvent
{
    public DateTimeOffset OccurredAt => PlacedAt;
}

// Infrastructure/Persistence/AppDbContext.cs
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var aggregates = ChangeTracker.Entries<AggregateRoot>()
        .Where(e => e.Entity.DomainEvents.Count > 0)
        .Select(e => e.Entity)
        .ToList();

    var events = aggregates.SelectMany(a => a.DomainEvents).ToList();

    var result = await base.SaveChangesAsync(ct);

    await _domainEventDispatcher.DispatchAsync(events, ct);

    foreach (var aggregate in aggregates)
        aggregate.ClearDomainEvents();

    return result;
}
```

### Domain Services

- Domain Services are simple, stateless classes. No existing any dependency injections
- Work with Domain Objects. Their methods can get and return entities, value objects... etc.

```csharp
// Domain/Orders/Services/OrderPricingService.cs
public sealed class OrderPricingService
{
    public Money CalculateFinalPrice(Order order, CustomerTier customerTier, ShippingZone shippingZone, Voucher? voucher = null)
    {
      var subtotal = order.Items.Aggregate(Money.Zero, (acc, item) => acc + item.SubTotal);

      var discountRate = GetDiscountRate(customerTier);
      var discountAmount = subtotal * discountRate;
      var priceAfterDiscount = subtotal - discountAmount;

      var shippingFee = CalculateShippinhFee(order, shippingZone);

      var voucherDiscount = voucher?.Apply(priceAfterDiscount) ?? Money.Zero;

      return priceAfterDiscount + shippingFee - voucherDiscount;
    }

    private static decimal GetDiscountRate(CustomerTier customerTier) => customerTier switch
    {
      CustomerTier.Silver => 0.05m, // 5%
      CustomerTier.Gold => 0.10m,   // 10%
      CustomerTier.Platinum => 0.15m, // 15%
      _ => 0m // Standard - no discount
    }
}
```

## Anti-patterns

### Time Control Pattern

**NEVER use `DateTime.Now` or `DateTimeOffset.UtcNow` directly in domain code!**

#### IClock Interface

```csharp
public interface IClock
{
    DateTimeOffset Now { get; }
}
```

#### Usage in Domain Code

```csharp
public void Complete(int completedByUserId, DateTimeOffset timestamp, IClock clock)
{
    Status = ShipperOrderStatusType.Delivered;

    var change = new ShipperOrderChangeEvent(
        timestamp,        // Business time (from command/user action)
        clock.Now,        // Audit time (when it was processed)
        completedByUserId,
        ShipperOrderStatusType.Delivered,
        ShipperOrderChangeEventType.StatusChanged,
        ShipperOrderChangeEventSourceType.Dispatcher);
    _changeEvents.Add(change);
}
```

**Benefits:**
- **Testable**: Inject mock clock for deterministic tests
- **Auditable**: Separate business time from processing time
- **Consistent**: All time handling centralized

### Oversized Aggregates

```csharp
// BAD — Customer aggregate owns everything the customer touches
public class Customer : AggregateRoot
{
    public List<Order> Orders { get; } = [];        // should be separate aggregate
    public List<Payment> Payments { get; } = [];     // should be separate aggregate
    public List<Address> Addresses { get; } = [];    // might be OK as child
    public ShoppingCart Cart { get; set; }            // should be separate aggregate
}

// GOOD — small, focused aggregates linked by ID
public class Customer : AggregateRoot
{
    public CustomerName Name { get; private set; }
    public EmailAddress Email { get; private set; }
    // Orders, Payments, Cart are separate aggregates referencing CustomerId
}
```

### Domain Events for Intra-Aggregate Logic

```csharp
// BAD — using events for logic within the same aggregate
order.RaiseDomainEvent(new OrderLineAdded(line));
// Then a handler recalculates the total... but you're in the same aggregate!

// GOOD — just call the method directly within the aggregate
_lines.Add(line);
RecalculateTotal();  // private method, no event needed
```

### Value Objects with Identity

```csharp
// BAD — value object with an Id (it's an entity then!)
public record Address
{
    public Guid Id { get; init; }  // value objects don't have identity
    public string Street { get; init; }
}

// GOOD — value objects are defined by their attributes, not an Id
public record Address(string Street, string City, string PostalCode, string Country);
```

### Anemic Aggregates

```csharp
// BAD — aggregate is just a data bag, service does all the work
public class Order : AggregateRoot
{
    public OrderStatus Status { get; set; }  // public setter!
    public List<OrderLine> Lines { get; set; } = [];
}

// Service directly manipulates order state
order.Status = OrderStatus.Confirmed;  // no invariant check!
order.Lines.Add(newLine);              // no validation!

// GOOD — aggregate encapsulates rules (see Aggregate Root pattern above)
order.Confirm();  // validates status, raises event
order.AddLine(productId, quantity, unitPrice);  // validates, recalculates
```

## Decision Guide

| Scenario | Recommendation |
|----------|---------------|
| When to use DDD | Complex domain with business rules that go beyond CRUD |
| When to use value objects | Any concept with validation rules or equality based on attributes, not identity |
| Aggregate size | Keep small — typically 1 root entity + 0-3 child entities. Load the whole aggregate every time |
| Domain events vs integration events | Domain events: within bounded context, same transaction. Integration events: cross-context, via message bus |
| Strongly-typed IDs | Always for aggregate root IDs that cross boundaries. Optional for child entity IDs |
| When NOT to use DDD | Simple CRUD, settings, audit logs, read models — use plain entities |
| Repository vs DbContext | Repository per aggregate root for complex aggregates; IAppDbContext for simpler queries |
| Domain services | Only when logic requires multiple aggregates or external data the aggregate should not know about |