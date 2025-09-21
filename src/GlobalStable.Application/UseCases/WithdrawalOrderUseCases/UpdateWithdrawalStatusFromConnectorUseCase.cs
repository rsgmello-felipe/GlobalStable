using FluentResults;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Helpers;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.WithdrawalOrderUseCases;

public class UpdateWithdrawalStatusFromConnectorUseCase(
    IWithdrawalOrderRepository withdrawalOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    IOrderEventPublisher orderEventPublisher,
    ILogger<UpdateWithdrawalStatusFromConnectorUseCase> logger)
{
    public async Task<Result> ExecuteAsync(ConnectorWithdrawalEvent eventMessage)
    {
        try
        {
            var withdrawalOrder = await withdrawalOrderRepository.GetByIdAsync(eventMessage.WithdrawalOrderId);
            if (withdrawalOrder == null)
            {
                logger.LogError("WithdrawalOrder not found. WithdrawalOrderId: {Id}", eventMessage.WithdrawalOrderId);
                return Result.Fail("WithdrawalOrder not found");
            }

            var newStatus = await orderStatusRepository.GetByNameAsync(eventMessage.Status);

            if (newStatus == null)
            {
                logger.LogError("Status not found. Status: {Status}", eventMessage.Status);
                return Result.Fail("Status not found");
            }

            if (!OrderStateValidator.ValidAutomaticWithdrawalTransition(withdrawalOrder.StatusId, newStatus.Id))
            {
                logger.LogCritical(
                    "Invalid status transition from {CurrentStatusId} to {NewStatusId} for WithdrawalOrderId {Id}",
                    withdrawalOrder.StatusId,
                    newStatus.Id,
                    withdrawalOrder.Id);

                return Result.Fail($"Invalid transition from status {withdrawalOrder.StatusId} to {newStatus.Id}");
            }

            withdrawalOrder.UpdateBankTransactionInformation(
                eventMessage.Reason,
                eventMessage.E2EId);

            withdrawalOrder.UpdateStatus(newStatus, "Callback", eventMessage.Reason);
            await withdrawalOrderRepository.UpdateAsync(withdrawalOrder);

            await orderEventPublisher.PublishWithdrawalOrderEvent(withdrawalOrder);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error processing withdrawal status update for WithdrawalOrderId {Id}", eventMessage.WithdrawalOrderId);
            return Result.Fail($"Error processing withdrawal status update: {ex.Message}");
        }
    }
}
