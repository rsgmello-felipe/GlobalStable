using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Helpers;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.Deposit;

public class UpdateDepositStatusFromConnectorUseCase(
    IDepositOrderRepository depositOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    IOrderEventPublisher orderEventPublisher,
    ILogger<UpdateDepositStatusFromConnectorUseCase> logger)
{
    public async Task<Result> ExecuteAsync(ConnectorDepositEvent eventMessage)
    {
        try
        {
            var depositOrder = await depositOrderRepository.GetByIdAsync(eventMessage.DepositOrderId);
            if (depositOrder == null)
            {
                logger.LogError("DepositOrder not found. DepositOrderId: {Id}", eventMessage.DepositOrderId);
                return Result.Fail("DepositOrder not found");
            }

            var newStatus = await orderStatusRepository.GetByNameAsync(eventMessage.Status);
            if (newStatus == null)
            {
                logger.LogError("Status not found. Status: {Status}", eventMessage.Status);
                return Result.Fail("Status not found");
            }

            if (newStatus.Name.Equals(OrderStatuses.PendingInBank))
            {
                return Result.Ok();
            }

            if (!OrderStateValidator.ValidAutomaticDepositTransition(depositOrder.StatusId, newStatus.Id))
            {
                logger.LogCritical(
                    "Invalid status transition from {CurrentStatusId} to {NewStatusId} for DepositOrderId {Id}",
                    depositOrder.StatusId,
                    newStatus.Id,
                    depositOrder.Id);

                return Result.Fail($"Invalid transition from status {depositOrder.StatusId} to {newStatus.Id}");
            }

            depositOrder.UpdateBankTransactionInformation(eventMessage.BankId, eventMessage.Reason, eventMessage.E2EId);
            depositOrder.UpdateStatus(newStatus, "Callback", eventMessage.Reason);
            await depositOrderRepository.UpdateAsync(depositOrder);

            await orderEventPublisher.PublishDepositOrderEvent(depositOrder);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error processing deposit status update for DepositOrderId {Id}", eventMessage.DepositOrderId);
            return Result.Fail($"Error processing deposit status update: {ex.Message}");
        }
    }
}
