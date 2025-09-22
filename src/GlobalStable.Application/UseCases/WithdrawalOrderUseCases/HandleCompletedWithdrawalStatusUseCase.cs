using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.Withdrawal;

/// <summary>
/// Handles the processing of pending withdrawal orders.
/// </summary>
public class HandleCompletedWithdrawalStatusUseCase(
    IWithdrawalOrderRepository withdrawalOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    IOrderHistoryRepository orderHistoryRepository,
    ITransactionEventPublisher transactionEventPublisher,
    ITransactionServiceClient transactionServiceClient,
    INotificationPublisher notificationPublisher,
    ILogger<HandleCompletedWithdrawalStatusUseCase> logger)
{
    public async Task<Result<WithdrawalOrderResponse>> ExecuteAsync(WithdrawalOrder withdrawalOrder)
    {
        try
        {
            logger.LogInformation("Starting confirmation for Withdrawal Order {OrderId}", withdrawalOrder.Id);


            var orderHistory = await orderHistoryRepository.GetWithdrawalOrderHistory(withdrawalOrder.Id);

            var previousStatusId = orderHistory
                .OrderByDescending(oh => oh.CreatedAt)
                .ElementAtOrDefault(1)?
                .StatusId;

            if (previousStatusId == null)
            {
                logger.LogCritical("No previous status for completed withdrawal. OrderId: {orderId}", withdrawalOrder.Id);
                return Result.Fail($"No previous status for completed withdrawal. OrderId: {withdrawalOrder.Id}");
            }

            var previousStatus = await orderStatusRepository.GetByIdAsync(previousStatusId.Value);

            if (previousStatus.Name.Equals(OrderStatuses.Processing))
            {
                // withdrawal Transaction
                await transactionEventPublisher.PublishTransactionCreatedAsync(
                    new CreateTransactionEvent(
                        withdrawalOrder.CustomerId,
                        withdrawalOrder.AccountId,
                        withdrawalOrder.Id,
                        nameof(TransactionType.Debit),
                        -Math.Abs(withdrawalOrder.RequestedAmount),
                        withdrawalOrder.Currency.Code,
                        nameof(TransactionOrderType.Withdrawal),
                        null,
                        withdrawalOrder.E2EId,
                        null));

                // Fee Transaction
                await transactionEventPublisher.PublishTransactionCreatedAsync(
                    new CreateTransactionEvent(
                        withdrawalOrder.CustomerId,
                        withdrawalOrder.AccountId,
                        withdrawalOrder.Id,
                        nameof(TransactionType.Debit),
                        -Math.Abs(withdrawalOrder.FeeAmount),
                        withdrawalOrder.Currency.Code,
                        nameof(TransactionOrderType.WithdrawalFee),
                        null,
                        withdrawalOrder.E2EId,
                        null));

                logger.LogInformation("Processing Withdrawal Order {OrderId}", withdrawalOrder.Id);
                return Result.Ok();
            }

            logger.LogCritical("Could not complete WithdrawalOrder. Invalid status sequence. OrderId: {orderId}", withdrawalOrder.Id);
            return Result.Fail($"Could not complete WithdrawalOrder. OrderId: {withdrawalOrder.Id}");
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "Critical error processing withdrawal order {OrderId}",
                withdrawalOrder.Id);

            return Result.Fail<WithdrawalOrderResponse>($"Error processing withdrawal order: {ex.Message}");
        }
    }
}