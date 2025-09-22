using FluentResults;
using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;
using GlobalStable.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GlobalStable.Application.UseCases.WithdrawalOrderUseCases;

/// <summary>
///     Handles the processing of pending withdrawal orders.
/// </summary>
public class HandleCreatedWithdrawalUseCase(
    IWithdrawalOrderRepository withdrawalOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    ITransactionServiceClient transactionServiceClient,
    IBrlProviderClient brlProviderClient,
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

            var processingStatus = await orderStatusRepository.GetByNameAsync(OrderStatuses.PendingInBank);
            if (processingStatus is null)
            {
                logger.LogError("Order status '{sentToConnector}' not found.", OrderStatuses.PendingInBank);
                return Result.Fail<WithdrawalOrderResponse>(
                    $"Order status '{OrderStatuses.PendingInBank}' not found.");
            }

            var withdrawalRequest = new BrlProviderCreateWithdrawalRequest(
                withdrawalOrder.Id.ToString(),
                withdrawalOrder.Currency.Code,
                withdrawalOrder.RequestedAmount,
                withdrawalOrder.ReceiverAccountKey,
                withdrawalOrder.ReceiverTaxId,
                withdrawalOrder.ReceiverName);

            var requestWithdrawalResponse = await brlProviderClient.CreateWithdrawalAsync(withdrawalRequest);

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