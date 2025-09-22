using System.Net;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using FakeItEasy;
using FluentAssertions;
using GlobalStable.Application.UseCases.DepositUseCases;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Refit;

namespace GlobalStable.Tests.UnitTests.Application.Deposit;

public class HandleDepositStatusUpdatedUseCaseTests
{
    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute()
            : base(() =>
            new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true })) { }
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenStatusIsCompleted_AndIsNotRefund_AndHandlerSucceeds(
        DepositOrder depositOrder,
        Currency currency,
        [Frozen] IDepositOrderRepository depositOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] ITransactionEventPublisher transactionEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ILogger<HandleCompletedDepositStatusUseCase> completedLogger,
        [Frozen] ILogger<HandleDepositStatusUpdatedUseCase> logger)
    {
        depositOrder = SetOrderStatus(depositOrder, 7); // statusId for "COMPLETED"
        depositOrder = SetCurrency(depositOrder, currency);

        A.CallTo(() => depositOrderRepository.GetByIdAsync(depositOrder.Id)).Returns(depositOrder);
        A.CallTo(() => orderHistoryRepository.GetDepositOrderHistory(depositOrder.Id)).Returns(new List<OrderHistory>
        {
            new OrderHistory(null, depositOrder.Id, OrderType.Deposit, 7, "test")
            {
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new OrderHistory(null, depositOrder.Id, OrderType.Deposit, 10, "test")
            {
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            },
        });

        A.CallTo(() => orderStatusRepository.GetByIdAsync(7))
            .Returns(new OrderStatus(7, OrderStatuses.Completed));

        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
        var deleteResponse = new ApiResponse<BaseApiResponse<DeletePendingTransactionResponse>>(
            httpResponse,
            new BaseApiResponse<DeletePendingTransactionResponse>(),
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.DeletePendingTransactionAsync(
                depositOrder.CustomerId,
                depositOrder.AccountId,
                depositOrder.Id))

            .Returns(deleteResponse);

        var handleCompleted = new HandleCompletedDepositStatusUseCase(
            orderStatusRepository,
            orderHistoryRepository,
            transactionEventPublisher,
            completedLogger,
            CreateInMemoryDb());

        var sut = new HandleDepositStatusUpdatedUseCase(
            depositOrderRepository,
            orderStatusRepository,
            notificationPublisher,
            logger,
            handleCompleted);

        var result = await sut.ExecuteAsync(new OrderEvent(depositOrder.Id));

        // assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => notificationPublisher.PublishDepositOrderFinishedAsync(
                A<DepositOrderNotificationEvent>._, MessagingKeys.NotificationDepositOrder))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenDepositOrderIsNotFound(
        long orderId,
        [Frozen] IDepositOrderRepository depositOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] ITransactionEventPublisher transactionEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ILogger<HandleCompletedDepositStatusUseCase> completedLogger,
        [Frozen] ILogger<HandleDepositStatusUpdatedUseCase> logger)
    {
        A.CallTo(() => depositOrderRepository.GetByIdAsync(orderId)).Returns((DepositOrder?)null);

        var handleCompleted = new HandleCompletedDepositStatusUseCase(
            orderStatusRepository,
            orderHistoryRepository,
            transactionEventPublisher,
            completedLogger,
            CreateInMemoryDb());

        var sut = new HandleDepositStatusUpdatedUseCase(
            depositOrderRepository,
            orderStatusRepository,
            notificationPublisher,
            logger,
            handleCompleted);

        var result = await sut.ExecuteAsync(new OrderEvent(orderId));

        result.IsFailed.Should().BeTrue();

        A.CallTo(() => notificationPublisher.PublishDepositOrderFinishedAsync(
                A<DepositOrderNotificationEvent>._, MessagingKeys.NotificationDepositOrder))
            .MustNotHaveHappened();

        result.Errors.First().Message.Should().Be("DepositOrder not found");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenExceptionIsThrown(
        DepositOrder depositOrder,
        [Frozen] IDepositOrderRepository depositOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] ITransactionEventPublisher transactionEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ILogger<HandleCompletedDepositStatusUseCase> completedLogger,
        [Frozen] ILogger<HandleDepositStatusUpdatedUseCase> logger)
    {
        A.CallTo(() => depositOrderRepository.GetByIdAsync(depositOrder.Id)).Throws(new Exception("boom"));

        var handleCompleted = new HandleCompletedDepositStatusUseCase(
            orderStatusRepository,
            orderHistoryRepository,
            transactionEventPublisher,
            completedLogger,
            CreateInMemoryDb());

        var sut = new HandleDepositStatusUpdatedUseCase(
            depositOrderRepository,
            orderStatusRepository,
            notificationPublisher,
            logger,
            handleCompleted);

        var result = await sut.ExecuteAsync(new OrderEvent(depositOrder.Id));

        result.IsFailed.Should().BeTrue();

        A.CallTo(() => notificationPublisher.PublishDepositOrderFinishedAsync(
                A<DepositOrderNotificationEvent>._, MessagingKeys.NotificationDepositOrder))
            .MustNotHaveHappened();

        result.Errors.First().Message.Should().Contain("boom");
    }

    private static ServiceDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseInMemoryDatabase("test_" + Guid.NewGuid())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ServiceDbContext(options);
    }

    private static DepositOrder SetOrderStatus(DepositOrder order, long statusId)
    {
        typeof(DepositOrder).GetProperty(nameof(DepositOrder.StatusId))!
            .SetValue(order, statusId);
        return order;
    }

    private DepositOrder SetCurrency(DepositOrder order, Currency currency)
    {
        var currencyProperty = typeof(DepositOrder).GetProperty("Currency");
        currencyProperty?.SetValue(order, currency);
        return order;
    }
}
