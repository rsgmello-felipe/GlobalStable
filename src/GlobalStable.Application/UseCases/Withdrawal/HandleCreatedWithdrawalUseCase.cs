using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;
using GlobalStable.Infrastructure.Settings;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GlobalStable.Application.UseCases.Withdrawal;

/// <summary>
///     Handles the processing of pending withdrawal orders.
/// </summary>
public class HandleCreatedWithdrawalUseCase(
    IWithdrawalOrderRepository withdrawalOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    ITransactionServiceClient transactionServiceClient,
    IBgpConnectorClient bgpConnectorClient,
    IOrderEventPublisher orderEventPublisher,
    IAccountRepository accountRepository,
    ILogger<HandleCreatedWithdrawalUseCase> logger,
    IOptions<CallbackSettings> callbackSettings)
{
    public async Task<Result<WithdrawalOrderResponse>> ExecuteAsync(WithdrawalOrder withdrawalOrder)
    {
        try
        {
            logger.LogInformation("Starting processing for Withdrawal Order {OrderId}", withdrawalOrder.Id);

            var account = await accountRepository.GetByIdAsync(withdrawalOrder.AccountId);

            # region MANUAL EXECUTION
            if (!account!.AutoExecuteWithdrawal)
            {
                var pendingTreasury = await orderStatusRepository.GetByNameAsync(OrderStatuses.PendingTreasury);
                if (pendingTreasury is null)
                {
                    logger.LogError("Order status '{pendingTreasury}' not found.", OrderStatuses.PendingTreasury);
                    return Result.Fail<WithdrawalOrderResponse>(
                        $"Order status '{OrderStatuses.PendingTreasury}' not found.");
                }

                withdrawalOrder.UpdateStatus(pendingTreasury, "ProcessCreatedWithdrawalConsumer");
                await withdrawalOrderRepository.UpdateAsync(withdrawalOrder);

                logger.LogInformation("Processing Withdrawal Order {OrderId}", withdrawalOrder.Id);
                return Result.Ok();
            }
            #endregion

            # region AUTOMATIC EXECUTION

            var processingStatus = await orderStatusRepository.GetByNameAsync(OrderStatuses.SentToConnector);
            if (processingStatus is null)
            {
                logger.LogError("Order status '{sentToConnector}' not found.", OrderStatuses.SentToConnector);
                return Result.Fail<WithdrawalOrderResponse>(
                    $"Order status '{OrderStatuses.SentToConnector}' not found.");
            }

            var withdrawalRequest = new BgpCreateWithdrawalRequest(
                withdrawalOrder.Id.ToString(),
                withdrawalOrder.Currency.Code,
                withdrawalOrder.RequestedAmount,
                withdrawalOrder.ReceiverAccountKey,
                withdrawalOrder.ReceiverTaxId,
                withdrawalOrder.Name.Split().First(),
                withdrawalOrder.Name.Split().Last(),
                callbackSettings.Value.WithdrawalUrl);

            var requestWithdrawalResponse = await bgpConnectorClient.CreateWithdrawalAsync(withdrawalRequest);

            if (!requestWithdrawalResponse.IsSuccessful)
            {
                logger.LogError("Error processing Withdrawal Order {OrderId} - BGP Error: {error}", withdrawalOrder.Id, requestWithdrawalResponse.Error?.Content);

                var failedStatus = await orderStatusRepository.GetByNameAsync(OrderStatuses.Failed);
                withdrawalOrder.UpdateStatus(failedStatus, "ProcessCreatedWithdrawalConsumer", "Provider failed to process withdrawal.");
                await withdrawalOrderRepository.UpdateAsync(withdrawalOrder);

                await orderEventPublisher.PublishWithdrawalOrderEvent(withdrawalOrder);
                return Result.Fail("Failed to create withdrawal request.");
            }

            withdrawalOrder.UpdateStatus(processingStatus, "ProcessCreatedWithdrawalConsumer");
            await withdrawalOrderRepository.UpdateAsync(withdrawalOrder);

            logger.LogInformation("Processing Withdrawal Order {OrderId}", withdrawalOrder.Id);
            return Result.Ok();

            #endregion
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