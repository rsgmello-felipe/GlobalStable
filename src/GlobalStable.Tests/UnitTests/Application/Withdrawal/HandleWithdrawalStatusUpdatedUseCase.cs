using System.Net;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using FakeItEasy;
using FluentAssertions;
using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.TransactionService;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using GlobalStable.Infrastructure.HttpClients.ApiResponses.BgpConnector;
using GlobalStable.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace GlobalStable.Tests.UnitTests.Application.Withdrawal;

public class HandleWithdrawalStatusUpdatedUseCase
{
    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute()
            : base(() =>
                new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true })) { }
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenOrderIsCreated_AndHandlerSucceeds(
        WithdrawalOrder withdrawalOrder,
        Currency currency,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IBgpConnectorClient bgpConnectorClient,
        [Frozen] IOptions<CallbackSettings> callbackSettings,
        [Frozen] ITransactionEventPublisher transactionEventPublisher,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<HandleCreatedWithdrawalUseCase> createdHandlerLogger,
        [Frozen] ILogger<HandleFailedWithdrawalUseCase> failedHandlerLogger,
        [Frozen] ILogger<HandleCompletedWithdrawalStatusUseCase> completedHandlerLogger,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] ILogger<GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase> logger)
    {
        // Arrange
        var status = new OrderStatus(1, "CREATED");
        withdrawalOrder = new WithdrawalOrder(
            customerId: 1,
            withdrawalOrder.AccountId,
            false,
            100,
            5,
            105,
            1,
            status.Id,
            "Test Test",
            origin: "",
            "E2E",
            "18997933728",
            "18997933728",
            null,
            null,
            "BR123",
            null,
            "tester");
        withdrawalOrder = SetCurrency(withdrawalOrder, currency);

        var createdHandler = new HandleCreatedWithdrawalUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            transactionServiceClient,
            bgpConnectorClient,
            orderEventPublisher,
            accountRepository,
            createdHandlerLogger,
            callbackSettings);

        var completedHandler = new HandleCompletedWithdrawalStatusUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderHistoryRepository,
            transactionEventPublisher,
            transactionServiceClient,
            notificationPublisher,
            completedHandlerLogger);

        var failedHandler = new HandleFailedWithdrawalUseCase(
            orderStatusRepository,
            transactionServiceClient,
            failedHandlerLogger);

        var sut = new GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            notificationPublisher,
            logger,
            createdHandler,
            failedHandler,
            completedHandler);

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

        A.CallTo(() => transactionServiceClient.GetBalanceAsync(
                withdrawalOrder.CustomerId,
                withdrawalOrder.AccountId))
            .Returns(Task.FromResult(balanceApiResponse));

        var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ }"),
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://fake-url.com"),
        };

        var successApiResponse = new ApiResponse<BaseApiResponse<CreatePendingTransactionResponse>>(
            fakeResponse,
            new BaseApiResponse<CreatePendingTransactionResponse>(),
            new RefitSettings(),
            null);

        A.CallTo(() => transactionServiceClient.CreatePendingTransactionAsync(
                A<CreatePendingTransactionRequest>._,
                A<long>._,
                A<long>._))
            .Returns(Task.FromResult(successApiResponse));

        var successWithdrawalBgp = new ApiResponse<BaseApiResponse<BgpCreateWithdrawalResponse>>(
            fakeResponse,
            new BaseApiResponse<BgpCreateWithdrawalResponse>(),
            new RefitSettings());

        A.CallTo(() => bgpConnectorClient.CreateWithdrawalAsync(A<BgpCreateWithdrawalRequest>._))
            .Returns(Task.FromResult(successWithdrawalBgp));

        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(withdrawalOrder.Id))
            .Returns(withdrawalOrder);

        A.CallTo(() => orderStatusRepository.GetByIdAsync(1))
            .Returns(new OrderStatus(1, OrderStatuses.Created));

        var @event = new OrderEvent(withdrawalOrder.Id);

        // Act
        var result = await sut.ExecuteAsync(@event);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => notificationPublisher.PublishWithdrawalOrderFinishedAsync(
                A<WithdrawalOrderNotificationEvent>._, MessagingKeys.NotificationWithdrawalOrder))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenOrderIsFailed_AndHandlerSucceeds(
        WithdrawalOrder withdrawalOrder,
        Currency currency,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IBgpConnectorClient bgpConnectorClient,
        [Frozen] IOptions<CallbackSettings> callbackSettings,
        [Frozen] ITransactionEventPublisher transactionEventPublisher,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<HandleCreatedWithdrawalUseCase> createdHandlerLogger,
        [Frozen] ILogger<HandleFailedWithdrawalUseCase> failedHandlerLogger,
        [Frozen] ILogger<HandleCompletedWithdrawalStatusUseCase> completedHandlerLogger,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] ILogger<GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase> logger)
    {
        // Arrange
        var status = new OrderStatus(8, OrderStatuses.Failed);

        withdrawalOrder = new WithdrawalOrder(
            customerId: 1,
            withdrawalOrder.AccountId,
            false,
            100,
            5,
            105,
            1,
            status.Id,
            "Test Test",
            origin: "",
            "E2E",
            "BANK",
            receiverTaxId: "18997933728",
            receiverAccountKey: "18997933728",
            receiverWalletAddress: null,
            blockchainNetworkId: null,
            null,
            "tester");
        withdrawalOrder = SetCurrency(withdrawalOrder, currency);

        var createdHandler = new HandleCreatedWithdrawalUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            transactionServiceClient,
            bgpConnectorClient,
            orderEventPublisher,
            accountRepository,
            createdHandlerLogger,
            callbackSettings);

        var completedHandler = new HandleCompletedWithdrawalStatusUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderHistoryRepository,
            transactionEventPublisher,
            transactionServiceClient,
            notificationPublisher,
            completedHandlerLogger);

        var failedHandler = new HandleFailedWithdrawalUseCase(
            orderStatusRepository,
            transactionServiceClient,
            failedHandlerLogger);

        var sut = new GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            notificationPublisher,
            logger,
            createdHandler,
            failedHandler,
            completedHandler);

        var deletePendingResponse = new ApiResponse<BaseApiResponse<DeletePendingTransactionResponse>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new BaseApiResponse<DeletePendingTransactionResponse>(),
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.DeletePendingTransactionAsync(
                withdrawalOrder.CustomerId,
                withdrawalOrder.AccountId,
                withdrawalOrder.Id))
            .Returns(Task.FromResult(deletePendingResponse));

        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(withdrawalOrder.Id))
            .Returns(withdrawalOrder);

        A.CallTo(() => orderStatusRepository.GetByIdAsync(8))
            .Returns(new OrderStatus(8, OrderStatuses.Failed));

        var @event = new OrderEvent(withdrawalOrder.Id);

        // Act
        var result = await sut.ExecuteAsync(@event);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => notificationPublisher.PublishWithdrawalOrderFinishedAsync(
                A<WithdrawalOrderNotificationEvent>._, MessagingKeys.NotificationWithdrawalOrder))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenWithdrawalOrderIsNull(
        long withdrawalOrderId,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] ILogger<GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase> logger)
    {
        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(withdrawalOrderId)).Returns((WithdrawalOrder)null);

        var useCase = new GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            notificationPublisher,
            logger,
            A.Dummy<HandleCreatedWithdrawalUseCase>(),
            A.Dummy<HandleFailedWithdrawalUseCase>(),
            A.Dummy<HandleCompletedWithdrawalStatusUseCase>());

        var result = await useCase.ExecuteAsync(new OrderEvent(withdrawalOrderId));

        result.IsFailed.Should().BeTrue();

        A.CallTo(() => notificationPublisher.PublishWithdrawalOrderFinishedAsync(
                A<WithdrawalOrderNotificationEvent>._, MessagingKeys.NotificationWithdrawalOrder))
            .MustNotHaveHappened();

        result.Errors.First().Message.Should().Be("WithdrawalOrder not found");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenUnexpectedExceptionIsThrown(
        OrderEvent eventMessage,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] ILogger<GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase> logger)
    {
        var exception = new InvalidOperationException("Unexpected failure");
        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(eventMessage.OrderId))
            .Throws(exception);

        var sut = new GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            notificationPublisher,
            logger,
            A.Dummy<HandleCreatedWithdrawalUseCase>(),
            A.Dummy<HandleFailedWithdrawalUseCase>(),
            A.Dummy<HandleCompletedWithdrawalStatusUseCase>());

        var result = await sut.ExecuteAsync(eventMessage);

        result.IsFailed.Should().BeTrue();

        A.CallTo(() => notificationPublisher.PublishWithdrawalOrderFinishedAsync(
                A<WithdrawalOrderNotificationEvent>._, MessagingKeys.NotificationWithdrawalOrder))
            .MustNotHaveHappened();

        result.Errors.First().Message.Should().Contain("Error processing withdrawal status update");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenCreatedHandlerFails(
        Currency currency,
        Account account,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IBgpConnectorClient bgpConnectorClient,
        [Frozen] IOptions<CallbackSettings> callbackSettings,
        [Frozen] ITransactionEventPublisher transactionEventPublisher,
        [Frozen] ILogger<HandleCreatedWithdrawalUseCase> createdHandlerLogger,
        [Frozen] ILogger<HandleFailedWithdrawalUseCase> failedHandlerLogger,
        [Frozen] ILogger<HandleCompletedWithdrawalStatusUseCase> completedHandlerLogger,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase> logger)
    {
        var status = new OrderStatus(1, OrderStatuses.Created);

        var withdrawalOrder = new WithdrawalOrder(
            customerId: 1,
            accountId: 123,
            isAutomated: false,
            requestedAmount: 100,
            feeAmount: 5,
            totalAmount: 105,
            currencyId: 1,
            statusId: status.Id,
            name: "Test",
            origin: "",
            e2eId: "E2E",
            bankId: "BANK",
            receiverTaxId: "18997933728",
            receiverAccountKey: "18997933728",
            receiverWalletAddress: null,
            blockchainNetworkId: null,
            webhookUrl: null,
            createdBy: "tester");

        SetCurrency(withdrawalOrder, currency);

        var createdHandler = new HandleCreatedWithdrawalUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            transactionServiceClient,
            bgpConnectorClient,
            orderEventPublisher,
            accountRepository,
            createdHandlerLogger,
            callbackSettings);

        var completedHandler = new HandleCompletedWithdrawalStatusUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderHistoryRepository,
            transactionEventPublisher,
            transactionServiceClient,
            notificationPublisher,
            completedHandlerLogger);

        var failedHandler = new HandleFailedWithdrawalUseCase(
            orderStatusRepository,
            transactionServiceClient,
            failedHandlerLogger);

        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(withdrawalOrder.Id)).Returns(withdrawalOrder);


        A.CallTo(() => orderStatusRepository.GetByIdAsync(1))
            .Returns(new OrderStatus(1, OrderStatuses.Created));

        var autoExecuteProperty = typeof(Account).GetProperty("AutoExecuteWithdrawal");
        autoExecuteProperty?.SetValue(account, true);

        A.CallTo(() => accountRepository.GetByIdAsync(withdrawalOrder.AccountId))
            .Returns(account);

        var fakeResponse = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("{ }"),
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://fake-url.com"),
        };

        var successWithdrawalBgp = new ApiResponse<BaseApiResponse<BgpCreateWithdrawalResponse>>(
            fakeResponse,
            new BaseApiResponse<BgpCreateWithdrawalResponse>(),
            new RefitSettings());

        A.CallTo(() => bgpConnectorClient.CreateWithdrawalAsync(A<BgpCreateWithdrawalRequest>._))
            .Returns(Task.FromResult(successWithdrawalBgp));

        var sut = new GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            notificationPublisher,
            logger,
            createdHandler,
            failedHandler,
            completedHandler);

        var result = await sut.ExecuteAsync(new OrderEvent(withdrawalOrder.Id));

        result.IsFailed.Should().BeTrue();

        A.CallTo(() => notificationPublisher.PublishWithdrawalOrderFinishedAsync(
                A<WithdrawalOrderNotificationEvent>._, MessagingKeys.NotificationWithdrawalOrder))
            .MustNotHaveHappened();

        result.Errors.First().Message.Should().Contain("Failed to create withdrawal request.");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenFailedHandlerFails(
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IAccountRepository accountRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderHistoryRepository orderHistoryRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] IBgpConnectorClient bgpConnectorClient,
        [Frozen] IOptions<CallbackSettings> callbackSettings,
        [Frozen] ITransactionEventPublisher transactionEventPublisher,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<HandleCreatedWithdrawalUseCase> createdHandlerLogger,
        [Frozen] ILogger<HandleFailedWithdrawalUseCase> failedHandlerLogger,
        [Frozen] ILogger<HandleCompletedWithdrawalStatusUseCase> completedHandlerLogger,
        [Frozen] INotificationPublisher notificationPublisher,
        [Frozen] ILogger<GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase> logger)
    {
        var status = new OrderStatus(8, OrderStatuses.Failed);

        var withdrawalOrder = new WithdrawalOrder(
            customerId: 1,
            accountId: 123,
            isAutomated: false,
            requestedAmount: 100,
            feeAmount: 5,
            totalAmount: 105,
            currencyId: 1,
            statusId: status.Id,
            name: "Test",
            origin: "",
            e2eId: "E2E",
            bankId: "BANK",
            receiverTaxId: "18997933728",
            receiverAccountKey: "18997933728",
            receiverWalletAddress: null,
            blockchainNetworkId: null,
            webhookUrl: null,
            createdBy: "tester");

        var createdHandler = new HandleCreatedWithdrawalUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            transactionServiceClient,
            bgpConnectorClient,
            orderEventPublisher,
            accountRepository,
            createdHandlerLogger,
            callbackSettings);

        var completedHandler = new HandleCompletedWithdrawalStatusUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderHistoryRepository,
            transactionEventPublisher,
            transactionServiceClient,
            notificationPublisher,
            completedHandlerLogger);

        var failedHandler = new HandleFailedWithdrawalUseCase(
            orderStatusRepository,
            transactionServiceClient,
            failedHandlerLogger);

        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(withdrawalOrder.Id)).Returns(withdrawalOrder);
        A.CallTo(() => orderStatusRepository.GetAllAsync()).Returns(new List<OrderStatus> { status });

        var failedResponse = new ApiResponse<BaseApiResponse<DeletePendingTransactionResponse>>(
            new HttpResponseMessage(HttpStatusCode.BadRequest),
            new BaseApiResponse<DeletePendingTransactionResponse>(),
            new RefitSettings());

        A.CallTo(() => orderStatusRepository.GetByIdAsync(8))
            .Returns(new OrderStatus(8, OrderStatuses.Failed));

        A.CallTo(() => transactionServiceClient.DeletePendingTransactionAsync(
                withdrawalOrder.CustomerId,
                withdrawalOrder.AccountId,
                withdrawalOrder.Id))
            .Returns(failedResponse);

        var sut = new GlobalStable.Application.UseCases.Withdrawal.HandleWithdrawalStatusUpdatedUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            notificationPublisher,
            logger,
            createdHandler,
            failedHandler,
            completedHandler);

        var result = await sut.ExecuteAsync(new OrderEvent(withdrawalOrder.Id));

        result.IsFailed.Should().BeTrue();

        A.CallTo(() => notificationPublisher.PublishWithdrawalOrderFinishedAsync(
                A<WithdrawalOrderNotificationEvent>._, MessagingKeys.NotificationWithdrawalOrder))
            .MustNotHaveHappened();

        result.Errors.First().Message.Should()
            .Be($"Could not delete Pending Transaction for Order {withdrawalOrder.Id}.");
    }

    private WithdrawalOrder SetCurrency(WithdrawalOrder order, Currency currency)
    {
        var currencyProperty = typeof(WithdrawalOrder).GetProperty("Currency");
        currencyProperty?.SetValue(order, currency);
        return order;
    }
}