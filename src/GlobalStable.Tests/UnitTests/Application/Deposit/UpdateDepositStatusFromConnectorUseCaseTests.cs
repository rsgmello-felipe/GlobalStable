using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;
using GlobalStable.Application.UseCases.DepositUseCases;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Tests.UnitTests.Application.Deposit;

public class UpdateDepositStatusFromConnectorUseCaseTests
{
    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute()
            : base(() =>
            new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true })) { }
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenDepositExistsAndStatusIsValid(
        DepositOrder depositOrder,
        ConnectorDepositEvent eventMessage,
        [Frozen] IDepositOrderRepository depositOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<UpdateDepositStatusFromConnectorUseCase> logger)
    {
        // Arrange
        eventMessage = new ConnectorDepositEvent
        {
            DepositOrderId = depositOrder.Id,
            Status = "COMPLETED",
            BankId = "BANK123",
            Reason = "OK",
            E2EId = "E2E123",
        };

        SetOrderStatus(depositOrder, 2);

        var newStatus = new OrderStatus(7, OrderStatuses.Completed);

        A.CallTo(() => depositOrderRepository.GetByIdAsync(eventMessage.DepositOrderId)).Returns(depositOrder);
        A.CallTo(() => orderStatusRepository.GetByNameAsync(OrderStatuses.Completed)).Returns(newStatus);

        var sut = new UpdateDepositStatusFromConnectorUseCase(
            depositOrderRepository,
            orderStatusRepository,
            orderEventPublisher,
            logger);

        // Act
        var result = await sut.ExecuteAsync(eventMessage);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => depositOrderRepository.UpdateAsync(depositOrder)).MustHaveHappenedOnceExactly();
        A.CallTo(() => orderEventPublisher.PublishDepositOrderEvent(depositOrder)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenDepositOrderIsNotFound(
        ConnectorDepositEvent eventMessage,
        [Frozen] IDepositOrderRepository depositOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<UpdateDepositStatusFromConnectorUseCase> logger)
    {
        A.CallTo(() => depositOrderRepository.GetByIdAsync(eventMessage.DepositOrderId)).Returns((DepositOrder?)null);

        var sut = new UpdateDepositStatusFromConnectorUseCase(
            depositOrderRepository,
            orderStatusRepository,
            orderEventPublisher,
            logger);

        var result = await sut.ExecuteAsync(eventMessage);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("DepositOrder not found");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenExceptionIsThrown(
        ConnectorDepositEvent eventMessage,
        [Frozen] IDepositOrderRepository depositOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<UpdateDepositStatusFromConnectorUseCase> logger)
    {
        A.CallTo(() => depositOrderRepository.GetByIdAsync(eventMessage.DepositOrderId))
            .Throws(new Exception("boom"));

        var sut = new UpdateDepositStatusFromConnectorUseCase(
            depositOrderRepository,
            orderStatusRepository,
            orderEventPublisher,
            logger);

        var result = await sut.ExecuteAsync(eventMessage);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Error processing deposit status update:");
    }

    private static void SetOrderStatus(DepositOrder order, long statusId)
    {
        typeof(DepositOrder).GetProperty(nameof(DepositOrder.StatusId))!
            .SetValue(order, statusId);
    }
}
