using System.Net;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using FakeItEasy;
using FluentAssertions;
using GlobalStable.Application.UseCases.WithdrawalOrderUseCases;
using Microsoft.Extensions.Logging;
using Refit;

namespace GlobalStable.Tests.UnitTests.Application.Withdrawal;

public class HandleFailedWithdrawalUseCaseTests
{
    public class CustomAutoDataAttribute() : AutoDataAttribute(() =>
        new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true }));

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenTransactionIsDeletedAndStatusIsUpdated(
        WithdrawalOrder withdrawalOrder,
        OrderStatus failedStatus,
        Currency currency,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ILogger<HandleFailedWithdrawalUseCase> logger)
    {
        // Arrange
        failedStatus = new OrderStatus(10, OrderStatuses.Failed);
        withdrawalOrder = SetCurrency(withdrawalOrder, currency);

        A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.Failed))!
            .Returns(Task.FromResult(failedStatus));

        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
        var deleteResponse = new ApiResponse<BaseApiResponse<DeletePendingTransactionResponse>>(
            httpResponse,
            new BaseApiResponse<DeletePendingTransactionResponse>(),
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.DeletePendingTransactionAsync(
                withdrawalOrder.CustomerId,
                withdrawalOrder.AccountId,
                withdrawalOrder.Id))
            .Returns(deleteResponse);

        var sut = new HandleFailedWithdrawalUseCase(
            orderStatusRepository,
            transactionServiceClient,
            logger);

        // Act
        var result = await sut.ExecuteAsync(withdrawalOrder);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenDeletePendingTransactionFails(
        WithdrawalOrder withdrawalOrder,
        OrderStatus failedStatus,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ILogger<HandleFailedWithdrawalUseCase> logger)
    {
        // Arrange
        failedStatus = new OrderStatus(1, OrderStatuses.Failed);

        A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.Failed))
            .Returns(failedStatus);

        var failedHttpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{}"),
            RequestMessage = new HttpRequestMessage(HttpMethod.Delete, "https://fake-url.com"),
        };

        var failedApiResponse = new ApiResponse<BaseApiResponse<DeletePendingTransactionResponse>>(
            failedHttpResponse,
            new BaseApiResponse<DeletePendingTransactionResponse>(),
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.DeletePendingTransactionAsync(
                withdrawalOrder.CustomerId,
                withdrawalOrder.AccountId,
                withdrawalOrder.Id))
            .Returns(failedApiResponse);

        var sut = new HandleFailedWithdrawalUseCase(
            orderStatusRepository,
            transactionServiceClient,
            logger);

        // Act
        var result = await sut.ExecuteAsync(withdrawalOrder);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be($"Could not delete Pending Transaction for Order {withdrawalOrder.Id}.");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenPendingTransactionIsDeletedSuccessfully(
        WithdrawalOrder withdrawalOrder,
        OrderStatus failedStatus,
        Currency currency,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] ITransactionServiceClient transactionServiceClient,
        [Frozen] ILogger<HandleFailedWithdrawalUseCase> logger)
    {
        // Arrange
        failedStatus = new OrderStatus(1, OrderStatuses.Failed);
        withdrawalOrder = SetCurrency(withdrawalOrder, currency);

        A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.Failed))
            .Returns(failedStatus);

        var successHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}"),
            RequestMessage = new HttpRequestMessage(HttpMethod.Delete, "https://fake-url.com"),
        };

        var successApiResponse = new ApiResponse<BaseApiResponse<DeletePendingTransactionResponse>>(
            successHttpResponse,
            new BaseApiResponse<DeletePendingTransactionResponse>(),
            new RefitSettings());

        A.CallTo(() => transactionServiceClient.DeletePendingTransactionAsync(
                withdrawalOrder.CustomerId,
                withdrawalOrder.AccountId,
                withdrawalOrder.Id))
            .Returns(Task.FromResult(successApiResponse));

        var sut = new HandleFailedWithdrawalUseCase(
            orderStatusRepository,
            transactionServiceClient,
            logger);

        // Act
        var result = await sut.ExecuteAsync(withdrawalOrder);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    private WithdrawalOrder SetCurrency(WithdrawalOrder order, Currency currency)
    {
        var currencyProperty = typeof(WithdrawalOrder).GetProperty("Currency");
        currencyProperty?.SetValue(order, currency);
        return order;
    }
}