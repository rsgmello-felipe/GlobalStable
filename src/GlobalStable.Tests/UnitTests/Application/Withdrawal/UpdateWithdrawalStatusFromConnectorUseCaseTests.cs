using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Tests.UnitTests.Application.Withdrawal;

public class UpdateWithdrawalStatusFromConnectorUseCaseTests
{
    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute()
            : base(() =>
            new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true })) { }
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldSucceed_WhenStatusTransitionIsValid(
        WithdrawalOrder withdrawalOrder,
        ConnectorWithdrawalEvent eventMessage,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<UpdateWithdrawalStatusFromConnectorUseCase> logger)
    {
        // Arrange
        var newStatus = new OrderStatus(4, OrderStatuses.PendingInBank);
        SetOrderStatus(withdrawalOrder, 3); // e.g., SENT_TO_CONNECTOR

        eventMessage = new ConnectorWithdrawalEvent
        {
            WithdrawalOrderId = withdrawalOrder.Id,
            Status = newStatus.Name,
            Reason = "Callback status update",
        };

        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(eventMessage.WithdrawalOrderId)).Returns(withdrawalOrder);

        A.CallTo(() => orderStatusRepository.GetByNameAsync(eventMessage.Status))
            .Returns(new OrderStatus(4, OrderStatuses.PendingInBank));

        var sut = new UpdateWithdrawalStatusFromConnectorUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderEventPublisher,
            logger);

        // Act
        var result = await sut.ExecuteAsync(eventMessage);

        // Assert
        result.IsSuccess.Should().BeTrue();
        A.CallTo(() => withdrawalOrderRepository.UpdateAsync(withdrawalOrder)).MustHaveHappenedOnceExactly();
        A.CallTo(() => orderEventPublisher.PublishWithdrawalOrderEvent(withdrawalOrder)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenWithdrawalOrderNotFound(
        ConnectorWithdrawalEvent eventMessage,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<UpdateWithdrawalStatusFromConnectorUseCase> logger)
    {
        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(eventMessage.WithdrawalOrderId)).Returns((WithdrawalOrder?)null);

        var sut = new UpdateWithdrawalStatusFromConnectorUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderEventPublisher,
            logger);

        var result = await sut.ExecuteAsync(eventMessage);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("WithdrawalOrder not found");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenNewStatusNotFound(
        WithdrawalOrder withdrawalOrder,
        ConnectorWithdrawalEvent eventMessage,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<UpdateWithdrawalStatusFromConnectorUseCase> logger)
    {
        SetOrderStatus(withdrawalOrder, 3);

        eventMessage = new ConnectorWithdrawalEvent
        {
            WithdrawalOrderId = withdrawalOrder.Id,
            Status = "UNKNOWN_STATUS",
            Reason = "Callback",
        };

        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(eventMessage.WithdrawalOrderId)).Returns(withdrawalOrder);
        A.CallTo(() => orderStatusRepository.GetByNameAsync(eventMessage.Status))
            .Returns(Task.FromResult<OrderStatus?>(null));

        var sut = new UpdateWithdrawalStatusFromConnectorUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderEventPublisher,
            logger);

        var result = await sut.ExecuteAsync(eventMessage);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Status not found");
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenTransitionIsInvalid(
        WithdrawalOrder withdrawalOrder,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<UpdateWithdrawalStatusFromConnectorUseCase> logger)
    {
        var currentStatusId = 6; // PROCESSING
        var invalidNewStatus = new OrderStatus(1, "CREATED");

        SetOrderStatus(withdrawalOrder, currentStatusId);

        var eventMessage = new ConnectorWithdrawalEvent
        {
            WithdrawalOrderId = withdrawalOrder.Id,
            Status = invalidNewStatus.Name,
            Reason = "Rollback"
        };

        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(withdrawalOrder.Id)).Returns(withdrawalOrder);
        A.CallTo(() => orderStatusRepository.GetAllAsync()).Returns(new List<OrderStatus> { invalidNewStatus });

        var sut = new UpdateWithdrawalStatusFromConnectorUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderEventPublisher,
            logger);

        var result = await sut.ExecuteAsync(eventMessage);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Invalid transition");
        A.CallTo(() => withdrawalOrderRepository.UpdateAsync(A<WithdrawalOrder>._)).MustNotHaveHappened();
    }

    [Theory]
    [CustomAutoData]
    public async Task ShouldFail_WhenExceptionIsThrown(
        ConnectorWithdrawalEvent eventMessage,
        [Frozen] IWithdrawalOrderRepository withdrawalOrderRepository,
        [Frozen] IOrderStatusRepository orderStatusRepository,
        [Frozen] IOrderEventPublisher orderEventPublisher,
        [Frozen] ILogger<UpdateWithdrawalStatusFromConnectorUseCase> logger)
    {
        A.CallTo(() => withdrawalOrderRepository.GetByIdAsync(eventMessage.WithdrawalOrderId))
            .Throws(new Exception("boom"));

        var sut = new UpdateWithdrawalStatusFromConnectorUseCase(
            withdrawalOrderRepository,
            orderStatusRepository,
            orderEventPublisher,
            logger);

        var result = await sut.ExecuteAsync(eventMessage);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Error processing withdrawal status update");
    }

    private static void SetOrderStatus(WithdrawalOrder order, long statusId)
    {
        typeof(WithdrawalOrder).GetProperty(nameof(WithdrawalOrder.StatusId))!
            .SetValue(order, statusId);
    }
}
