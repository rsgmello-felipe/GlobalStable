using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.Deposit;

public class HandleDepositStatusUpdatedUseCase(
    IDepositOrderRepository depositOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    INotificationPublisher notificationPublisher,
    ILogger<HandleDepositStatusUpdatedUseCase> logger,
    HandleCompletedDepositStatusUseCase handleCompletedDepositStatusUseCase)
{
    public async Task<Result> ExecuteAsync(OrderEvent eventMessage)
    {
        try
        {
            var depositOrder = await depositOrderRepository.GetByIdAsync(eventMessage.OrderId);
            if (depositOrder is null)
            {
                logger.LogError("DepositOrder not found. DepositOrderId: {Id}", eventMessage.OrderId);
                return Result.Fail("DepositOrder not found");
            }

            var currentOrderStatus = await orderStatusRepository.GetByIdAsync(depositOrder.StatusId);

            if (currentOrderStatus.Name.Equals(OrderStatuses.Completed))
            {
                var result = await handleCompletedDepositStatusUseCase.ExecuteAsync(depositOrder);
                if (result.IsFailed)
                {
                    logger.LogError(
                        "DepositOrder status not be handled. OrderId: {orderId} - Status: {status}",
                        eventMessage.OrderId,
                        currentOrderStatus);
                    return Result.Fail(result.Errors.FirstOrDefault()?.Message);
                }
            }

            var notification = new DepositOrderNotificationEvent(
                depositOrder.Id,
                nameof(TransactionOrderType.Deposit),
                depositOrder.AccountId,
                depositOrder.CustomerId,
                depositOrder.WebhookUrl,
                depositOrder.RequestedAmount,
                depositOrder.Currency.Code,
                currentOrderStatus.Name,
                DateTimeOffset.UtcNow,
                depositOrder.StatusDescription);

            await notificationPublisher.PublishDepositOrderFinishedAsync(
                notification, MessagingKeys.NotificationDepositOrder);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error processing deposit status update for DepositOrderId {Id}", eventMessage.OrderId);
            return Result.Fail($"Error processing deposit status update: {ex.Message}");
        }
    }
}
