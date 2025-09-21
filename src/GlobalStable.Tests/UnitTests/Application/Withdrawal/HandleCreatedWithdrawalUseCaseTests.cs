using System.Net;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.TransactionService;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using GlobalStable.Infrastructure.HttpClients.ApiResponses.BgpConnector;
using GlobalStable.Infrastructure.Settings;
using FakeItEasy;
using FluentAssertions;
using GlobalStable.Application.UseCases.WithdrawalOrderUseCases;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;
using StackExchange.Redis;

namespace GlobalStable.Tests.UnitTests.Application.Withdrawal;

public class HandleCreatedWithdrawalUseCaseTests
{
    public class CustomAutoDataAttribute() : AutoDataAttribute(() =>
        new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true }));

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenHasSufficientBalance(
        WithdrawalOrder withdrawalOrder,
        Currency currency,
        OrderStatus processingStatus,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IBrlProviderClient brlProviderClient,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOptions<CallbackSettings> callbackSettings,
        [Frozen] ILogger<HandleCreatedWithdrawalUseCase> logger)
    {
        // Arrange
        processingStatus = new OrderStatus(1, "PROCESSING");
        withdrawalOrder = new WithdrawalOrder(
            customerId: 1,
            accountId: 123,
            requestedAmount: 100,
            feeAmount: 5,
            totalAmount: 105,
            currencyId: 1,
            statusId: 99,
            name: "Test",
            e2eId: "123456789",
            receiverAccountKey: "18997933728",
            receiverTaxId: "18997933728",
            receiverWalletAddress: null,
            blockchainNetworkId: null,
            createdBy: "tester");

        withdrawalOrder = SetCurrency(withdrawalOrder, currency);

        A.CallTo(() => orderStatusRepository.GetByNameAsync("PROCESSING"))!
            .Returns(Task.FromResult(processingStatus));

        var balanceResponse = new GetBalanceResponse
        {
            AccountId = withdrawalOrder.AccountId,
            Balance = withdrawalOrder.TotalAmount + 100,
        };

        var balanceBaseApiResponse = new BaseApiResponse<GetBalanceResponse>(balanceResponse);

        var balanceApiResponse = new ApiResponse<BaseApiResponse<GetBalanceResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            balanceBaseApiResponse,
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.GetBalanceAsync(withdrawalOrder.CustomerId, withdrawalOrder.AccountId))
            .Returns(balanceApiResponse);

        var responseContent = new CreatePendingTransactionResponse
        {
            Id = 1234,
            AccountId = 123,
            Type = "Debit",
            Amount = 100,
            Currency = "BRL",
            OrderId = withdrawalOrder.Id,
            OrderType = "Withdrawal",
        };

        var baseApiResponse = new BaseApiResponse<CreatePendingTransactionResponse>(responseContent);

        var successApiResponse = new ApiResponse<BaseApiResponse<CreatePendingTransactionResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            baseApiResponse,
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.CreatePendingTransactionAsync(A<CreatePendingTransactionRequest>._, A<long>._, A<long>._))
            .Returns(Task.FromResult(successApiResponse));

        var successWithdrawalBgp = new ApiResponse<BaseApiResponse<BrlProviderCreateWithdrawalResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<BrlProviderCreateWithdrawalResponse>(),
            new RefitSettings(),
            null);

        A.CallTo(() => brlProviderClient.CreateWithdrawalAsync(A<BrlProviderCreateWithdrawalRequest>._))
            .Returns(Task.FromResult(successWithdrawalBgp));

        var sut = new HandleCreatedWithdrawalUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            transactionServiceClient,
            brlProviderClient,
            orderEventPublisher,
            accountRepository,
            logger,
            callbackSettings);

        // Act
        var result = await sut.ExecuteAsync(withdrawalOrder);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenProcessingStatusIsNotFound(
        WithdrawalOrder withdrawalOrder,
        Account account,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IBrlProviderClient brlProviderClient,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOptions<CallbackSettings> callbackSettings,
        [Frozen] ILogger<HandleCreatedWithdrawalUseCase> logger)
    {
        A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.SentToConnector))
            .Returns(Task.FromResult<OrderStatus?>(null));

        var autoExecuteProperty = typeof(Account).GetProperty("AutoExecuteWithdrawal");
        autoExecuteProperty?.SetValue(account, true);

        A.CallTo(() => accountRepository.GetByIdAsync(withdrawalOrder.AccountId))
            .Returns(account);

        var sut = new HandleCreatedWithdrawalUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            transactionServiceClient,
            brlProviderClient,
            orderEventPublisher,
            accountRepository,
            logger,
            callbackSettings);

        var result = await sut.ExecuteAsync(withdrawalOrder);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be($"Order status '{OrderStatuses.SentToConnector}' not found.");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceedWithFailedStatus_WhenInsufficientBalance(
        WithdrawalOrder withdrawalOrder,
        OrderStatus processingStatus,
        OrderStatus failedStatus,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IBrlProviderClient brlProviderClient,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOptions<CallbackSettings> callbackSettings,
        [Frozen] ILogger<HandleCreatedWithdrawalUseCase> logger)
    {
        processingStatus = new OrderStatus(1, OrderStatuses.Processing);
        failedStatus = new OrderStatus(2, OrderStatuses.Failed);

        A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.Processing))!
            .Returns(Task.FromResult(processingStatus));

        A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.Failed))!
            .Returns(Task.FromResult(failedStatus));

        var balanceResponse = new GetBalanceResponse
        {
            AccountId = withdrawalOrder.AccountId,
            Balance = 0,
        };

        var balanceBaseApiResponse = new BaseApiResponse<GetBalanceResponse>(balanceResponse);

        var balanceApiResponse = new ApiResponse<BaseApiResponse<GetBalanceResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            balanceBaseApiResponse,
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.GetBalanceAsync(withdrawalOrder.CustomerId, withdrawalOrder.AccountId))
            .Returns(Task.FromResult(balanceApiResponse));

        var sut = new HandleCreatedWithdrawalUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            transactionServiceClient,
            brlProviderClient,
            orderEventPublisher,
            accountRepository,
            logger,
            callbackSettings);

        var result = await sut.ExecuteAsync(withdrawalOrder);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenExceptionIsThrown(
        WithdrawalOrder withdrawalOrder,
        Account account,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IBrlProviderClient brlProviderClient,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOptions<CallbackSettings> callbackSettings,
        [Frozen] ILogger<HandleCreatedWithdrawalUseCase> logger)
    {
        A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.Processing))
            .Throws(new Exception("Unexpected error"));

        var autoExecuteProperty = typeof(Account).GetProperty("AutoExecuteWithdrawal");
        autoExecuteProperty?.SetValue(account, true);

        A.CallTo(() => accountRepository.GetByIdAsync(withdrawalOrder.AccountId))
            .Returns(account);

        var sut = new HandleCreatedWithdrawalUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            transactionServiceClient,
            brlProviderClient,
            orderEventPublisher,
            accountRepository,
            logger,
            callbackSettings);

        var result = await sut.ExecuteAsync(withdrawalOrder);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Error processing withdrawal order");
    }

    private WithdrawalOrder SetCurrency(WithdrawalOrder order, Currency currency)
    {
        var currencyProperty = typeof(WithdrawalOrder).GetProperty("Currency");
        currencyProperty?.SetValue(order, currency);
        return order;
    }
}