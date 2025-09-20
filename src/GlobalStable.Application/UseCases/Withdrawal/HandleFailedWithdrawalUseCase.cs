using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.TransactionService;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.Withdrawal;

/// <summary>
/// Handles the processing of pending withdrawal orders.
/// </summary>
public class HandleFailedWithdrawalUseCase(
    IOrderStatusRepository orderStatusRepository,
    ITransactionServiceClient transactionServiceClient,
    ILogger<HandleFailedWithdrawalUseCase> logger)
{
    public async Task<Result<WithdrawalOrderResponse>> ExecuteAsync(WithdrawalOrder withdrawalOrder)
    {
        try
        {
            logger.LogInformation("Starting confirmation for Withdrawal Order {OrderId}", withdrawalOrder.Id);

            var response = await transactionServiceClient.DeletePendingTransactionAsync(
                withdrawalOrder.CustomerId,
                withdrawalOrder.AccountId,
                withdrawalOrder.Id);

            if (!response.IsSuccessful)
            {
                logger.LogCritical("Could not delete Pending Transaction for Order {OrderId}.", withdrawalOrder.Id);
                return Result.Fail<WithdrawalOrderResponse>(
                    $"Could not delete Pending Transaction for Order {withdrawalOrder.Id}.");
            }

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