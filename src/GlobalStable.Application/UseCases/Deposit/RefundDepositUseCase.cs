using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.TransactionService;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.Deposit;

public class RefundDepositUseCase(
    IDepositOrderRepository depositOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    ITransactionServiceClient transactionServiceClient,
    IBgpConnectorClient bgpConnectorClient,
    ILogger<RefundDepositUseCase> logger)
{
    public async Task<Result<RefundDepositResponse>> ExecuteAsync(long orderId, long accountId)
    {
        try
        {
            var depositOrder = await depositOrderRepository.GetByIdWithAccountAsync(orderId, accountId);
            if (depositOrder == null)
            {
                return Result.Fail<RefundDepositResponse>("DepositOrder not found");
            }

            var completedStatus = await orderStatusRepository.GetByNameAsync(OrderStatuses.Completed);

            if (completedStatus == null)
            {
                logger.LogCritical("Status '{completed}' not found", OrderStatuses.Completed);
                return Result.Fail<RefundDepositResponse>($"Status '{OrderStatuses.Completed}' not found");
            }

            if (depositOrder.StatusId != completedStatus.Id)
            {
                logger.LogError("Deposit cannot be refunded because it is not in status '{completed}'.", OrderStatuses.Completed);
                return Result.Fail<RefundDepositResponse>($"Deposit cannot be refunded because it is not in status '{OrderStatuses.Completed}'.");
            }

            var refundRequestedStatus = await orderStatusRepository.GetByNameAsync(OrderStatuses.ProcessingRefund);
            if (refundRequestedStatus == null)
            {
                logger.LogCritical("Status '{processingRefund}' not found", OrderStatuses.ProcessingRefund);
                return Result.Fail<RefundDepositResponse>($"Status '{OrderStatuses.ProcessingRefund}' not found");
            }

            depositOrder.UpdateStatus(refundRequestedStatus, "API");

            var pendingRefundTransaction = new CreatePendingTransactionRequest(
                depositOrder.AccountId,
                -Math.Abs(depositOrder.RequestedAmount),
                depositOrder.Currency.Code,
                depositOrder.Id,
                TransactionType.Debit,
                nameof(TransactionOrderType.DepositReturned));

            var response = await transactionServiceClient.CreatePendingTransactionAsync(
                pendingRefundTransaction,
                depositOrder.CustomerId,
                depositOrder.AccountId);

            if (!response.IsSuccessful)
            {
                logger.LogCritical("Failed to create pending refund transaction for depositOrderId: {orderId}", depositOrder.Id);
                return Result.Fail<RefundDepositResponse>("Failed to create pending refund transaction");
            }

            var bgpResponse = await bgpConnectorClient.RefundDepositAsync(orderId);
            if (!bgpResponse.IsSuccessful)
            {
                logger.LogCritical("Failed to request refund in BGP Connector for depositOrderId: {orderId}", depositOrder.Id);
                return Result.Fail<RefundDepositResponse>("Failed request refund in BGP Connector ");
            }

            await depositOrderRepository.UpdateAsync(depositOrder);

            var result = new RefundDepositResponse(
                depositOrder.Id,
                refundRequestedStatus.Name,
                depositOrder.LastUpdatedAt);

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error while processing refund for depositOrderId: {orderId}", orderId);
            return Result.Fail<RefundDepositResponse>("Error processing refund");
        }
    }
}
