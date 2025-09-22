using System.Net;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Refit;
using Xunit;

namespace GlobalStable.Tests.UnitTests.Application.Withdrawal;

public class HandleCompletedWithdrawalStatusUseCaseTests
{
    public class CustomAutoDataAttribute() : AutoDataAttribute(() =>
        new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true }));

    private static List<OrderHistory> CreateOrderedHistory(long orderId, long currentStatusId, long previousStatusId)
    {
        var now = DateTimeOffset.UtcNow;
        return new List<OrderHistory>
        {
            new OrderHistory(orderId, null, OrderType.Withdrawal, currentStatusId, "test") { CreatedAt = now },
            new OrderHistory(orderId, null, OrderType.Withdrawal, previousStatusId, "test") { CreatedAt = now.AddMilliseconds(-1) },
        };
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldCreateTransactionsAndNotify_WhenPreviousStatusIsProcessing(
        WithdrawalOrder withdrawalOrder,
        Currency currency,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ITransactionEventPublisher transactionPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] ILogger<HandleCompletedWithdrawalStatusUseCase> logger)
    {
        // Arrange
        var statuses = new List<OrderStatus>
        {
            new(7, OrderStatuses.Completed),
            new(6, OrderStatuses.Processing),
        };

        withdrawalOrder = SetCurrency(withdrawalOrder, currency);

        A.CallTo(() => orderHistoryRepository.GetWithdrawalOrderHistory(withdrawalOrder.Id))
            .Returns(CreateOrderedHistory(withdrawalOrder.Id, 7, 6));

        A.CallTo(() => orderStatusRepository.GetByIdAsync(6))
            .Returns(new OrderStatus(6, OrderStatuses.Processing));


        var sut = new HandleCompletedWithdrawalStatusUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderHistoryRepository,
            transactionPublisher,
            transactionServiceClient,
            notificationPublisher,
            logger);

        // Act
        var result = await sut.ExecuteAsync(withdrawalOrder);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => transactionPublisher.PublishTransactionCreatedAsync(
            A<CreateTransactionEvent>.That.Matches(e =>
                e.Type == nameof(TransactionType.Debit) &&
                e.Amount == -Math.Abs(withdrawalOrder.RequestedAmount))))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => transactionPublisher.PublishTransactionCreatedAsync(
            A<CreateTransactionEvent>.That.Matches(e =>
                e.Type == nameof(TransactionType.Debit) &&
                e.Amount == -Math.Abs(withdrawalOrder.FeeAmount))))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenPreviousStatusIsNotAllowed(
        WithdrawalOrder withdrawalOrder,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ITransactionEventPublisher transactionPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] ILogger<HandleCompletedWithdrawalStatusUseCase> logger)
    {
        // Arrange
        var statuses = new List<OrderStatus>
        {
            new(7, OrderStatuses.Completed),
            new(13, OrderStatuses.Created),
        };

        A.CallTo(() => orderStatusRepository.GetAllAsync()).Returns(statuses);
        A.CallTo(() => orderHistoryRepository.GetWithdrawalOrderHistory(withdrawalOrder.Id))
            .Returns(CreateOrderedHistory(withdrawalOrder.Id, 7, 13));

        A.CallTo(() => orderStatusRepository.GetByIdAsync(13))
            .Returns(new OrderStatus(13, OrderStatuses.Created));


        var sut = new HandleCompletedWithdrawalStatusUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderHistoryRepository,
            transactionPublisher,
            transactionServiceClient,
            notificationPublisher,
            logger);

        // Act
        var result = await sut.ExecuteAsync(withdrawalOrder);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Could not complete WithdrawalOrder");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenExceptionIsThrown(
        WithdrawalOrder withdrawalOrder,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ITransactionEventPublisher transactionPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] ILogger<HandleCompletedWithdrawalStatusUseCase> logger)
    {
        // Arrange
        A.CallTo(() => orderHistoryRepository.GetWithdrawalOrderHistory(withdrawalOrder.Id))
            .Throws(new Exception("unexpected error"));

        var sut = new HandleCompletedWithdrawalStatusUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderHistoryRepository,
            transactionPublisher,
            transactionServiceClient,
            notificationPublisher,
            logger);

        // Act
        var result = await sut.ExecuteAsync(withdrawalOrder);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Error processing withdrawal order");
    }

    private WithdrawalOrder SetCurrency(WithdrawalOrder order, Currency currency)
    {
        var currencyProperty = typeof(WithdrawalOrder).GetProperty("Currency");
        currencyProperty?.SetValue(order, currency);
        return order;
    }
}
