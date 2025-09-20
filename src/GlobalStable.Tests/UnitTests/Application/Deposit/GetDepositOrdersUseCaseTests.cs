using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Application.UseCases.Deposit;
using GlobalStable.Domain.Common;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Tests.UnitTests.Application.Deposit;

public class GetDepositOrdersUseCaseTests
{
    private readonly IDepositOrderRepository _depositOrderRepository;
    private readonly IOrderStatusRepository _orderStatusRepository;
    private readonly GetDepositOrdersUseCase _useCase;

    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute()
            : base(() =>
                new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true })) { }
    }

    public GetDepositOrdersUseCaseTests()
    {
        _depositOrderRepository = A.Fake<IDepositOrderRepository>();
        _orderStatusRepository = A.Fake<IOrderStatusRepository>();
        ILogger<GetDepositOrdersUseCase> logger = A.Fake<ILogger<GetDepositOrdersUseCase>>();
        _useCase = new GetDepositOrdersUseCase(logger, _depositOrderRepository, _orderStatusRepository);
    }

    [Theory]
    [CustomAutoData]
    public async Task ExecuteAsync_ShouldReturnOk_WhenOrdersExist(Currency currency)
    {
        // Arrange
        var statusId = 1L;

        A.CallTo(() => _orderStatusRepository.GetStatusIdByNameAsync("COMPLETED"))
            .Returns(statusId);

        var depositOrder = new DepositOrder(
            customerId: 100,
            accountId: 200,
            isAutomated: false,
            requestedAmount: 1000,
            feeAmount: 10,
            totalAmount: 990,
            currencyId: currency.Id,
            statusId: statusId,
            bankReference: "REF123",
            webhookUrl: "https://example.com/webhook",
            expireAt: DateTimeOffset.UtcNow.AddHours(1),
            createdBy: "unit_test",
            origin: "",
            bankId: "BANK1",
            payerTaxId: "12345678900",
            pixCopyPaste: "pixcode",
            cvu: "CVU123",
            e2eId: "E2E123",
            statusDescription: "Completed",
            name: "Test Name");

        typeof(DepositOrder).GetProperty(nameof(DepositOrder.Currency))!
            .SetValue(depositOrder, currency);

        A.CallTo(() => _depositOrderRepository.GetFilteredAsync(
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
            .Returns(new PagedResult<DepositOrder>(
                new List<DepositOrder> { depositOrder },
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
            "accountId",
            "DESC",
            1,
            10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Orders.Should().HaveCount(1);
        result.Value.Orders.First().Currency.Should().Be(currency.Code);
        result.Value.Orders.First().Name.Should().Be("Test Name");
    }

    [Fact]
    [CustomAutoData]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenNoOrdersFound()
    {
        // Arrange
        A.CallTo(() => _orderStatusRepository.GetStatusIdByNameAsync("PENDING"))
            .Returns(2L);

        A.CallTo(() => _depositOrderRepository.GetFilteredAsync(
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
            .Returns(new PagedResult<DepositOrder>(
                new List<DepositOrder>(),
                new Pagination(0, 1, 10)));

        // Act
        var result = await _useCase.ExecuteAsync(
            1,
            1,
            1,
            "PENDING",
            null,
            null,
            null,
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