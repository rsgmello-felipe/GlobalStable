using FluentResults;
using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.WithdrawalOrderUseCases;

public class HandleWithdrawalStatusUpdatedUseCase(
    IWithdrawalOrderRepository withdrawalOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    INotificationPublisher notificationPublisher,
    ILogger<HandleWithdrawalStatusUpdatedUseCase> logger,
    HandleCreatedWithdrawalUseCase handleCreatedWithdrawalUseCase,
    HandleFailedWithdrawalUseCase handleFailedWithdrawalUseCase,
    HandleCompletedWithdrawalStatusUseCase handleCompletedWithdrawalStatusUseCase)
{
    public async Task<Result> ExecuteAsync(OrderEvent eventMessage)
    {
        try
        {
            var withdrawalOrder = await withdrawalOrderRepository.GetByIdAsync(eventMessage.OrderId);
            if (withdrawalOrder == null)
            {
                logger.LogError("WithdrawalOrder not found. WithdrawalOrderId: {Id}", eventMessage.OrderId);
                return Result.Fail("WithdrawalOrder not found");
            }

            var currentOrderStatus = await orderStatusRepository.GetByIdAsync(withdrawalOrder.StatusId);
            if (currentOrderStatus == null)
            {
                logger.LogError("OrderStatus not found. WithdrawalOrderId: {Id}", eventMessage.OrderId);
                return Result.Fail("OrderStatus not found");
            }

            if (currentOrderStatus.Name.Equals(OrderStatuses.Created))
            {
                var result = await handleCreatedWithdrawalUseCase.ExecuteAsync(withdrawalOrder);
                if (result.IsFailed)
                {
                    logger.LogError(
                        "WithdrawalOrder status could not be handled. OrderId: {orderId} - Status: {status}",
                        eventMessage.OrderId,
                        currentOrderStatus);
                    return Result.Fail(result.Errors.FirstOrDefault()?.Message);
                }
            }
            else if (currentOrderStatus.Name.Equals(OrderStatuses.Completed))
            {
                var result = await handleCompletedWithdrawalStatusUseCase.ExecuteAsync(withdrawalOrder);
                if (result.IsFailed)
                {
                    logger.LogError(
                        "WithdrawalOrder status could not be handled. OrderId: {orderId} - Status: {status}",
                        eventMessage.OrderId,
                        currentOrderStatus);
                    return Result.Fail(result.Errors.FirstOrDefault()?.Message);
                }
            }
            else if (currentOrderStatus.Name.Equals(OrderStatuses.Failed) ||
                     currentOrderStatus.Name.Equals(OrderStatuses.Rejected))
            {
                var result = await handleFailedWithdrawalUseCase.ExecuteAsync(withdrawalOrder);
                if (result.IsFailed)
                {
                    logger.LogError(
                        "WithdrawalOrder status could not be handled. OrderId: {orderId} - Status: {status}",
                        eventMessage.OrderId,
                        currentOrderStatus);
                    return Result.Fail(result.Errors.FirstOrDefault()?.Message);
                }
            }

            var notification = new WithdrawalOrderNotificationEvent(
                withdrawalOrder.Id,
                nameof(TransactionOrderType.Withdrawal),
                withdrawalOrder.AccountId,
                withdrawalOrder.CustomerId,
                withdrawalOrder.RequestedAmount,
                withdrawalOrder.Currency.Code,
                currentOrderStatus.Name,
                DateTimeOffset.UtcNow,
                withdrawalOrder.StatusDescription);

            await notificationPublisher.PublishWithdrawalOrderFinishedAsync(
                notification,
                MessagingKeys.NotificationWithdrawalOrder);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error processing withdrawal status update for WithdrawalOrderId {Id}", eventMessage.OrderId);
            return Result.Fail($"Error processing withdrawal status update: {ex.Message}");
        }
    }
}
