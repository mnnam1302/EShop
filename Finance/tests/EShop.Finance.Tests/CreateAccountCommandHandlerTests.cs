using EShop.Finance.Application.UseCases.CreateFinanceAccount;
using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.Account;
using EShop.Finance.Domain.Aggregates.Account.Commands;
using EShop.Finance.Domain.Enums;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.DomainTools.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EShop.Finance.Tests;

public class CreateAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IEventBus> _eventBus = new();

    private CreateAccountCommandHandler CreateHandler() => new(
        _repository.Object,
        _unitOfWork.Object,
        _eventBus.Object,
        NullLogger<CreateAccountCommandHandler>.Instance);

    private static CreateAccountCommand Command(decimal total = 120.00m, string frequency = PaymentFrequency.Quarterly) => new()
    {
        OrderId = Guid.NewGuid(),
        BuyerId = "buyer-1",
        TotalAmount = total,
        Currency = "USD",
        PaymentFrequency = frequency,
        TenantId = "tenant-1",
        ActionUserId = "user-1",
        ActionUserType = "System",
    };

    [Fact]
    public async Task Creates_account_schedules_payments_and_replies_scheduled()
    {
        _repository.Setup(r => r.FindByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);
        OrderPaymentScheduled? reply = null;
        _eventBus.Setup(b => b.PublishAsync(It.IsAny<OrderPaymentScheduled>(), It.IsAny<CancellationToken>()))
            .Callback<OrderPaymentScheduled, CancellationToken>((e, _) => reply = e)
            .Returns(Task.CompletedTask);

        var command = Command();
        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.Add(It.Is<Account>(a => a.OrderId == command.OrderId)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        reply.Should().NotBeNull();
        reply!.OrderId.Should().Be(command.OrderId);
        reply.PaymentCount.Should().Be(4); // Quarterly
        reply.TenantId.Should().Be("tenant-1");
    }

    [Fact]
    public async Task Invalid_total_replies_schedule_failed_and_does_not_persist()
    {
        _repository.Setup(r => r.FindByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);
        OrderPaymentScheduleFailed? reply = null;
        _eventBus.Setup(b => b.PublishAsync(It.IsAny<OrderPaymentScheduleFailed>(), It.IsAny<CancellationToken>()))
            .Callback<OrderPaymentScheduleFailed, CancellationToken>((e, _) => reply = e)
            .Returns(Task.CompletedTask);

        var command = Command(total: 0m);
        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _repository.Verify(r => r.Add(It.IsAny<Account>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        reply.Should().NotBeNull();
        reply!.OrderId.Should().Be(command.OrderId);
        reply.Reason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Existing_account_is_idempotent_and_re_replies_scheduled()
    {
        var command = Command();
        var existing = Account.Create(command.OrderId, command.BuyerId, command.TotalAmount, command.Currency, command.PaymentFrequency, command.TenantId);
        existing.CalculateScheduledPayments(new DateOnly(2026, 1, 15));
        _repository.Setup(r => r.FindByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        OrderPaymentScheduled? reply = null;
        _eventBus.Setup(b => b.PublishAsync(It.IsAny<OrderPaymentScheduled>(), It.IsAny<CancellationToken>()))
            .Callback<OrderPaymentScheduled, CancellationToken>((e, _) => reply = e)
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.Add(It.IsAny<Account>()), Times.Never);
        reply.Should().NotBeNull();
        reply!.AccountId.Should().Be(existing.Id);
    }
}
