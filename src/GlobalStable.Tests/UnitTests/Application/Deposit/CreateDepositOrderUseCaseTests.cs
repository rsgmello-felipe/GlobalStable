// using System.Net;
// using AutoFixture;
// using AutoFixture.AutoFakeItEasy;
// using AutoFixture.Xunit2;
// using FakeItEasy;
// using FluentAssertions;
// using GlobalStable.Application.ApiRequests;
// using GlobalStable.Application.UseCases.DepositUseCases;
// using GlobalStable.Domain.Constants;
// using GlobalStable.Domain.Entities;
// using GlobalStable.Domain.Enums;
// using GlobalStable.Domain.Interfaces.Repositories;
// using GlobalStable.Infrastructure.HttpClients;
// using GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;
// using GlobalStable.Infrastructure.HttpClients.ApiResponses;
// using GlobalStable.Infrastructure.HttpClients.ApiResponses.BgpConnector;
// using GlobalStable.Infrastructure.Persistence;
// using GlobalStable.Infrastructure.Settings;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Diagnostics;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using Refit;
//
// namespace GlobalStable.Tests.UnitTests.Application.Deposit;
//
// public class CreateDepositOrderUseCaseTests
// {
//     public class CustomAutoDataAttribute() : AutoDataAttribute(() =>
//         new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true }));
//
//     [Theory]
//     [CustomAutoData]
//     public async Task ShouldSucceed_WhenAllDataIsValid(
//         CreateDepositOrderRequest request,
//         Account account,
//         OrderStatus pendingStatus,
//         FeeConfig feeConfig,
//         BrlProviderCreateDepositResponse connectorResponse,
//         long accountId,
//         [Frozen] IRepository<DepositOrder> depositOrderRepository,
//         [Frozen] IOrderStatusRepository orderStatusRepository,
//         [Frozen] IFeeConfigRepository feeConfigRepository,
//         [Frozen] IAccountRepository accountRepository,
//         [Frozen] IBrlProviderClient brlProviderClient,
//         [Frozen] IOptions<CallbackSettings> callbackSettings,
//         [Frozen] ILogger<CreateDepositOrderUseCase> logger)
//     {
//         // Arrange
//         request.Amount = 100;
//         request.Expiration = 300;
//
//         account.Currency = new Currency(1, "BRL", "Brazilian Real", 2, CurrencyType.Fiat);
//
//         pendingStatus = new OrderStatus(1, OrderStatuses.PendingDeposit);
//
//         feeConfig = new FeeConfig(
//             accountId: accountId,
//             transactionOrderType: TransactionOrderType.Deposit,
//             feePercentage: 0.01m,
//             flatFee: 2,
//             createdBy: "tester");
//
//         connectorResponse = new BrlProviderCreateDepositResponse
//         {
//             Amount = request.Amount,
//             Currency = "BRL",
//             PixCopyPaste = "PIX123",
//             ExpireAt = DateTime.UtcNow.AddSeconds(request.Expiration),
//             Method = "PIX",
//             OrderId = "ORDER123",
//             ReferenceId = "REF123",
//         };
//
//         var baseApiResponse = new BaseApiResponse<BrlProviderCreateDepositResponse>(connectorResponse);
//
//         var apiResponse = new ApiResponse<BaseApiResponse<BrlProviderCreateDepositResponse>>(
//             new HttpResponseMessage(HttpStatusCode.OK),
//             baseApiResponse,
//             new RefitSettings(),
//             null);
//
//         A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.PendingDeposit))
//             .Returns(pendingStatus);
//
//         A.CallTo(() => feeConfigRepository.GetByAccountIdAsync(accountId, TransactionOrderType.Deposit))
//             .Returns(feeConfig);
//
//         A.CallTo(() => callbackSettings.Value)
//             .Returns(new CallbackSettings { DepositUrl = "https://callback.url/deposit" });
//
//         A.CallTo(() => brlProviderClient.CreateDepositOrderAsync(A<BrlProviderCreateDepositRequest>._))
//             .Returns(apiResponse);
//
//         A.CallTo(() => accountRepository.GetByIdAsync(accountId))
//             .Returns(account);
//
//         var sut = new CreateDepositOrderUseCase(
//             depositOrderRepository,
//             orderStatusRepository,
//             feeConfigRepository,
//             accountRepository,
//             brlProviderClient,
//             logger,
//             callbackSettings,
//             CreateInMemoryDb());
//
//         // Act
//         var result = await sut.ExecuteAsync(request, accountId, "user-123", null);
//
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         result.Value.RequestedAmount.Should().Be(100);
//         result.Value.TotalAmount.Should().Be(103);
//     }
//
//     [Theory]
//     [CustomAutoData]
//     public async Task ShouldFail_WhenPendingStatusNotFound(
//         CreateDepositOrderRequest request,
//         long accountId,
//         [Frozen] IOrderStatusRepository orderStatusRepository,
//         [Frozen] IFeeConfigRepository feeConfigRepository,
//         [Frozen] IRepository<DepositOrder> depositOrderRepository,
//         [Frozen] IAccountRepository accountRepository,
//         [Frozen] IBrlProviderClient brlProviderClient,
//         [Frozen] IOptions<CallbackSettings> callbackSettings,
//         [Frozen] ILogger<CreateDepositOrderUseCase> logger)
//     {
//         A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.PendingDeposit))
//             .Returns(Task.FromResult<OrderStatus?>(null));
//
//         var sut = new CreateDepositOrderUseCase(
//             depositOrderRepository,
//             orderStatusRepository,
//             feeConfigRepository,
//             accountRepository,
//             brlProviderClient,
//             logger,
//             callbackSettings,
//             CreateInMemoryDb());
//
//         var result = await sut.ExecuteAsync(request, accountId, "user-123", null);
//
//         result.IsFailed.Should().BeTrue();
//         result.Errors[0].Message.Should().Be($"Status '{OrderStatuses.PendingDeposit}' not found.");
//     }
//
//     [Theory]
//     [CustomAutoData]
//     public async Task ShouldFail_WhenFeeConfigNotFound(
//         CreateDepositOrderRequest request,
//         OrderStatus pendingStatus,
//         long accountId,
//         [Frozen] IOrderStatusRepository orderStatusRepository,
//         [Frozen] IFeeConfigRepository feeConfigRepository,
//         [Frozen] IRepository<DepositOrder> depositOrderRepository,
//         [Frozen] IAccountRepository accountRepository,
//         [Frozen] IBrlProviderClient brlProviderClient,
//         [Frozen] IOptions<CallbackSettings> callbackSettings,
//         [Frozen] ILogger<CreateDepositOrderUseCase> logger)
//     {
//         A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.PendingDeposit))
//             .Returns(pendingStatus);
//
//         A.CallTo(() => feeConfigRepository.GetByAccountIdAsync(accountId, TransactionOrderType.Deposit))
//             .Returns((FeeConfig?)null);
//
//         var sut = new CreateDepositOrderUseCase(
//             depositOrderRepository,
//             orderStatusRepository,
//             feeConfigRepository,
//             accountRepository,
//             brlProviderClient,
//             logger,
//             callbackSettings,
//             CreateInMemoryDb());
//
//         var result = await sut.ExecuteAsync(request, accountId, "user-123", null);
//
//         result.IsFailed.Should().BeTrue();
//         result.Errors[0].Message.Should().Contain("Fee config not found.");
//     }
//
//     [Theory]
//     [CustomAutoData]
//     public async Task ShouldFail_WhenExceptionThrown(
//         CreateDepositOrderRequest request,
//         long accountId,
//         [Frozen] IOrderStatusRepository orderStatusRepository,
//         [Frozen] IFeeConfigRepository feeConfigRepository,
//         [Frozen] IRepository<DepositOrder> depositOrderRepository,
//         [Frozen] IAccountRepository accountRepository,
//         [Frozen] IBrlProviderClient brlProviderClient,
//         [Frozen] IOptions<CallbackSettings> callbackSettings,
//         [Frozen] ILogger<CreateDepositOrderUseCase> logger)
//     {
//         A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.PendingDeposit))
//             .Throws(new Exception("Unexpected error"));
//
//         var sut = new CreateDepositOrderUseCase(
//             depositOrderRepository,
//             orderStatusRepository,
//             feeConfigRepository,
//             accountRepository,
//             brlProviderClient,
//             logger,
//             callbackSettings,
//             CreateInMemoryDb());
//
//         var result = await sut.ExecuteAsync(request, accountId, "user-123", null);
//
//         result.IsFailed.Should().BeTrue();
//         result.Errors[0].Message.Should().Contain("Error creating deposit order");
//     }
//
//     private static ServiceDbContext CreateInMemoryDb()
//     {
//         var options = new DbContextOptionsBuilder<ServiceDbContext>()
//             .UseInMemoryDatabase("test_" + Guid.NewGuid())
//             .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
//             .Options;
//
//         return new ServiceDbContext(options);
//     }
//
//     private DepositOrder SetCurrency(DepositOrder order, Currency currency)
//     {
//         var currencyProperty = typeof(DepositOrder).GetProperty("Currency");
//         currencyProperty?.SetValue(order, currency);
//         return order;
//     }
// }
