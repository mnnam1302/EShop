using EShop.Order.Domain.Commands;
using EShop.Order.Domain.Sagas;
using EShop.Order.Domain.StateMachines;
using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Inventory;
using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Exceptions;
using FluentAssertions;
using Moq;

namespace EShop.Order.Tests;

public class OrderSagaTests
{
    private readonly CapturingDispatcher _dispatcher = new();
    private readonly CapturingBus _bus = new();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly UserData _user = new("user-1", "user-1", "tenant-1");

    private OrderSaga NewSaga()
    {
        var provider = new Mock<IUserDetailsProvider>();
        provider.Setup(p => p.AuthenticatedUser).Returns(_user);

        var orderCreated = new OrderCreated
        {
            OrderId = _orderId,
            BuyerId = "buyer-1",
            Items = new List<OrderItem> { new() { VariantId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100m } },
            SubmittedAt = DateTimeOffset.UtcNow,
            TenantId = "tenant-1",
            ActionUserId = "user-1",
            ActionUserType = "System",
        };

        return OrderSaga.Create(OrderSagaId.FromOrderId(_orderId), orderCreated, provider.Object);
    }

    private async Task DrainAsync(OrderSaga saga)
    {
        await saga.PublishAsync(_dispatcher, CancellationToken.None);
        await saga.PublishAsync(_bus, CancellationToken.None);
    }

    private void ClearCaptured()
    {
        _dispatcher.Commands.Clear();
        _bus.Commands.Clear();
    }

    private InventoryReserved Reserved() => new()
    {
        OrderId = _orderId,
        ReservationId = Guid.NewGuid(),
        TenantId = "tenant-1",
        ActionUserId = "user-1",
        ActionUserType = "System",
    };

    [Fact]
    public async Task InventoryReserved_moves_to_processing_payment_and_stays_running()
    {
        var saga = NewSaga();
        await DrainAsync(saga); // flush MakeReservation raised at creation
        ClearCaptured();

        saga.HandleAsync(Reserved(), _user);
        await DrainAsync(saga);

        saga.IsCompleted().Should().BeFalse();
        saga.State.State.Should().Be(OrderSagaState.ProcessingPayment);
        _dispatcher.Commands.Should().ContainSingle(c => c is StartOrderPaymentCommand);
        _bus.Commands.Should().ContainSingle(c => c is MakePayment);
    }

    [Fact]
    public async Task PaymentScheduled_completes_saga_and_confirms_order_and_reservation()
    {
        var saga = NewSaga();
        saga.HandleAsync(Reserved(), _user);
        await DrainAsync(saga);
        ClearCaptured();

        saga.HandleAsync(new OrderPaymentScheduled
        {
            OrderId = _orderId,
            AccountId = Guid.NewGuid(),
            PaymentCount = 4,
            TenantId = "tenant-1",
            ActionUserId = "user-1",
            ActionUserType = "System",
        }, _user);
        await DrainAsync(saga);

        saga.IsCompleted().Should().BeTrue();
        saga.State.State.Should().Be(OrderSagaState.Completed);
        _dispatcher.Commands.Should().ContainSingle(c => c is AcceptOrderCommand);
        _bus.Commands.Should().ContainSingle(c => c is ConfirmReservationCommand);
    }

    [Fact]
    public async Task PaymentScheduleFailed_fails_saga_and_rejects_order_and_releases_reservation()
    {
        var saga = NewSaga();
        saga.HandleAsync(Reserved(), _user);
        await DrainAsync(saga);
        ClearCaptured();

        saga.HandleAsync(new OrderPaymentScheduleFailed
        {
            OrderId = _orderId,
            Reason = "invalid total",
            TenantId = "tenant-1",
            ActionUserId = "user-1",
            ActionUserType = "System",
        }, _user);
        await DrainAsync(saga);

        saga.IsCompleted().Should().BeTrue();
        saga.State.State.Should().Be(OrderSagaState.Failed);
        _dispatcher.Commands.Should().ContainSingle(c => c is RejectOrderCommand);
        _bus.Commands.Should().ContainSingle(c => c is ReleaseReservationCommand);
    }

    [Fact]
    public void PaymentScheduled_before_inventory_reserved_is_rejected()
    {
        var saga = NewSaga(); // state ReservingInventory

        var act = () => saga.HandleAsync(new OrderPaymentScheduled
        {
            OrderId = _orderId,
            AccountId = Guid.NewGuid(),
            PaymentCount = 1,
            TenantId = "tenant-1",
            ActionUserId = "user-1",
            ActionUserType = "System",
        }, _user);

        act.Should().Throw<DomainException>();
    }

    private sealed class CapturingDispatcher : ICommandDispatcher
    {
        public List<object> Commands { get; } = new();

        public Task<Result> DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand
        {
            Commands.Add(command!);
            return Task.FromResult(Result.Success());
        }

        public Task<Result<TResult>> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TResult> => throw new NotImplementedException();
    }

    private sealed class CapturingBus : ICommandBus
    {
        public List<object> Commands { get; } = new();

        public Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : IIntegrationCommand
        {
            Commands.Add(command!);
            return Task.CompletedTask;
        }
    }
}
