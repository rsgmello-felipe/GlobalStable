using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Domain.Common;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Tests.UnitTests.Application.Withdrawal;

public class GetWithdrawalOrdersUseCaseTests
{
    private readonly IWithdrawalOrderRepository _withdrawalOrderRepository;
    private readonly IOrderStatusRepository _orderStatusRepository;
    private readonly GetWithdrawalOrdersUseCase _useCase;

    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute()
            : base(() =>
                new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true }))
        {
        }
    }

    public GetWithdrawalOrdersUseCaseTests()
    {
        _withdrawalOrderRepository = A.Fake<IWithdrawalOrderRepository>();
        _orderStatusRepository = A.Fake<IOrderStatusRepository>();
        ILogger<GetWithdrawalOrdersUseCase> logger = A.Fake<ILogger<GetWithdrawalOrdersUseCase>>();
        _useCase = new GetWithdrawalOrdersUseCase(logger, _withdrawalOrderRepository, _orderStatusRepository);
    }

    [Theory]
    [CustomAutoData]
    public async Task ExecuteAsync_ShouldReturnOk_WhenOrdersExist(Currency currency)
    {
        // Arrange
        var statusId = 1L;

        A.CallTo(() => _orderStatusRepository.GetStatusIdByNameAsync("COMPLETED"))
            .Returns(statusId);

        var withdrawalOrder = new WithdrawalOrder(
            customerId: 100,
            accountId: 200,
            isAutomated: true,
            requestedAmount: 1000,
            feeAmount: 10,
            totalAmount: 990,
            currencyId: currency.Id,
            statusId: statusId,
            name: "Test Name",
            origin: "",
            e2eId: "E2E123",
            bankId: "Genial",
            webhookUrl: "https://example.com/webhook",
            createdBy: "unit_test",
            receiverTaxId: "22500432807",
            receiverAccountKey: "22500432807",
            receiverWalletAddress: null,
            receiverBlockchain: null);

        typeof(WithdrawalOrder).GetProperty(nameof(WithdrawalOrder.Currency))!
            .SetValue(withdrawalOrder, currency);

        A.CallTo(() => _withdrawalOrderRepository.GetFilteredAsync(
            A<long>._,
            A<long?>._,
            A<long?>._,
            A<long?>._,
            A<string>._,
            A<string>._,
            A<string>._,
            A<DateTime>._,
            A<DateTime>._,
            A<string>._,
            A<string>._,
            A<int>._,
            A<int>._))
            .Returns(new PagedResult<WithdrawalOrder>(
                new List<WithdrawalOrder> { withdrawalOrder },
                new Pagination(1, 10, 1)));

        A.CallTo(() => _orderStatusRepository.GetAllAsDictionaryAsync())
            .Returns(new Dictionary<long, string> { { statusId, "COMPLETED" } });

        // Act
        var result = await _useCase.ExecuteAsync(
            200,
            1,
            100,
            "COMPLETED",
            "Test Name",
            "E2E123",
            "18997933728",
            DateTime.Now.AddDays(-60),
            DateTime.Now,
            "AccountId",
            "DESC",
            1,
            10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Orders.Should().HaveCount(1);
        result.Value.Orders.First().Currency.Should().Be(currency.Code);
        result.Value.Orders.First().ReceiverName.Should().Be("Test Name");
    }

    [Fact]
    [CustomAutoData]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenNoOrdersFound()
    {
        // Arrange
        A.CallTo(() => _orderStatusRepository.GetStatusIdByNameAsync("PENDING"))
            .Returns(2L);

        A.CallTo(() => _withdrawalOrderRepository.GetFilteredAsync(
            A<long>._,
            A<long?>._,
            A<long?>._,
            A<long?>._,
            A<string>._,
            A<string>._,
            A<string>._,
            A<DateTime>._,
            A<DateTime>._,
            A<string>._,
            A<string>._,
            A<int>._,
            A<int>._))
            .Returns(new PagedResult<WithdrawalOrder>(
                new List<WithdrawalOrder>(),
                new Pagination(0, 1, 10)));

        // Act
        var result = await _useCase.ExecuteAsync(
            1,
            1,
            1,
            "PENDING",
            null,
            null,
            "18997933728",
            DateTime.Now.AddDays(-60),
            DateTime.Now,
            "accountId",
            "DESC",
            1,
            10);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}