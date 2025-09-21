using System.Net;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using GlobalStable.Infrastructure.Persistence;
using FakeItEasy;
using FluentAssertions;
using GlobalStable.Application.UseCases.DepositUseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Refit;

namespace GlobalStable.Tests.UnitTests.Application.Deposit;

public class HandleCompletedDepositStatusUseCaseTests
{
    public class CustomAutoDataAttribute() : AutoDataAttribute(() =>
        new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true }));

    private static DepositOrder CreateTestOrder(int statusId = 2)
    {
        return new DepositOrder(
            customerId: 1,
            accountId: 123,
            requestedAmount: 100,
            feeAmount: 5,
            totalAmount: 105,
            currencyId: 1,
            statusId: statusId,
            bankReference: "BR123",
            expireAt: DateTimeOffset.UtcNow.AddSeconds(300),
            createdBy: "test",
            e2eId: "E2E123");
    }

    private static ServiceDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseInMemoryDatabase("test_" + Guid.NewGuid())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ServiceDbContext(options);
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldDeletePendingTransaction_WhenPreviousStatusIsProcessingRefund(
        [Frozen] IDepositOrderRepository depositOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ITransactionEventPublisher transactionEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ILogger<HandleCompletedDepositStatusUseCase> logger)
    {
        var depositOrder = CreateTestOrder();
        var statusList = new List<OrderStatus>
        {
            new(7, OrderStatuses.Completed),
            new(10, OrderStatuses.ProcessingRefund),
        };

        A.CallTo(() => orderStatusRepository.GetAllAsync()).Returns(statusList);
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

        A.CallTo(() => orderStatusRepository.GetByIdAsync(10))
            .Returns(new OrderStatus(10, OrderStatuses.ProcessingRefund));

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

        var sut = new HandleCompletedDepositStatusUseCase(
            depositOrderRepository,
            orderStatusRepository,
            orderHistoryRepository,
            transactionEventPublisher,
            transactionServiceClient,
            logger,
            CreateInMemoryDb());

        var result = await sut.ExecuteAsync(depositOrder);

        result.IsSuccess.Should().BeTrue();
        A.CallTo(() => transactionServiceClient.DeletePendingTransactionAsync(
                depositOrder.CustomerId,
                depositOrder.AccountId,
                depositOrder.Id))

            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldCreateTransactions_WhenPreviousStatusIsPendingDeposit(
        Currency currency,
        [Frozen] IDepositOrderRepository depositOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ITransactionEventPublisher transactionEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ILogger<HandleCompletedDepositStatusUseCase> logger)
    {
        var depositOrder = CreateTestOrder();
        depositOrder = SetCurrency(depositOrder, currency);
        var statusList = new List<OrderStatus>
        {
            new(7, OrderStatuses.Completed),
            new(2, OrderStatuses.PendingDeposit)
        };

        var now = DateTimeOffset.UtcNow;

        A.CallTo(() => orderHistoryRepository.GetDepositOrderHistory(depositOrder.Id)).Returns(new List<OrderHistory>
        {
            new OrderHistory(null, depositOrder.Id, OrderType.Deposit, 7, "test")
            {
                CreatedAt = now,
            },
            new OrderHistory(null, depositOrder.Id, OrderType.Deposit, 2, "test")
            {
                CreatedAt = now.AddMinutes(-1),
            },
        });

        A.CallTo(() => orderStatusRepository.GetByIdAsync(2))
            .Returns(new OrderStatus(5, OrderStatuses.PendingDeposit));

        var sut = new HandleCompletedDepositStatusUseCase(
            depositOrderRepository,
            orderStatusRepository,
            orderHistoryRepository,
            transactionEventPublisher,
            transactionServiceClient,
            logger,
            CreateInMemoryDb());

        var result = await sut.ExecuteAsync(depositOrder);

        result.IsSuccess.Should().BeTrue();
        A.CallTo(() => transactionEventPublisher.PublishTransactionCreatedAsync(A<CreateTransactionEvent>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenPreviousStatusIsInvalid(
        [Frozen] IDepositOrderRepository depositOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ITransactionEventPublisher transactionEventPublisher,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ILogger<HandleCompletedDepositStatusUseCase> logger)
    {
        var depositOrder = CreateTestOrder();
        var statusList = new List<OrderStatus>
        {
            new(7, OrderStatuses.Completed),
            new(999, "SOME_OTHER_STATUS"),
        };

        A.CallTo(() => orderStatusRepository.GetAllAsync()).Returns(statusList);
        A.CallTo(() => orderHistoryRepository.GetWithdrawalOrderHistory(depositOrder.Id)).Returns(new List<OrderHistory>
        {
            new OrderHistory(null, depositOrder.Id, OrderType.Deposit, 7, "test")
            {
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new OrderHistory(
                null,
                depositOrder.Id,
                OrderType.Deposit,
                999,
                "test")
            {
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            },
        });

        var sut = new HandleCompletedDepositStatusUseCase(
            depositOrderRepository,
            orderStatusRepository,
            orderHistoryRepository,
            transactionEventPublisher,
            transactionServiceClient,
            logger,
            CreateInMemoryDb());

        var result = await sut.ExecuteAsync(depositOrder);

        result.IsFailed.Should().BeTrue();
    }

    private DepositOrder SetCurrency(DepositOrder order, Currency currency)
    {
        var currencyProperty = typeof(DepositOrder).GetProperty("Currency");
        currencyProperty?.SetValue(order, currency);
        return order;
    }
}
