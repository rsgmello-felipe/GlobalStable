using FluentResults;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.DepositUseCases;

public class HandleCompletedDepositStatusUseCase(
    IDepositOrderRepository depositOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    IOrderHistoryRepository orderHistoryRepository,
    ITransactionEventPublisher transactionEventPublisher,
    ITransactionServiceClient transactionServiceClient,
    ILogger<HandleCompletedDepositStatusUseCase> logger,
    ServiceDbContext dbContext)
{
    public async Task<Result> ExecuteAsync(DepositOrder depositOrder)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {

            var orderHistory = await orderHistoryRepository.GetDepositOrderHistory(depositOrder.Id);

            var previousStatusId = orderHistory
                .OrderByDescending(oh => oh.CreatedAt)
                .ElementAtOrDefault(1)?
                .StatusId;

            if (previousStatusId == null)
            {
                logger.LogCritical("No previous status for completed deposit. OrderId: {orderId}", depositOrder.Id);
                return Result.Fail($"No previous status for completed deposit. OrderId: {depositOrder.Id}");
            }

            var previousStatus = await orderStatusRepository.GetByIdAsync(previousStatusId.Value);

            switch (previousStatus.Name)
            {
                //REFUND ERROR
                case OrderStatuses.ProcessingRefund:
                    await transactionServiceClient.DeletePendingTransactionAsync(
                        depositOrder.CustomerId,
                        depositOrder.AccountId,
                        depositOrder.Id);

                    return Result.Ok();
                case OrderStatuses.PendingDeposit:
                    // Credit Transaction
                    await transactionEventPublisher.PublishTransactionCreatedAsync(
                        new CreateTransactionEvent(
                            depositOrder.CustomerId,
                            depositOrder.AccountId,
                            depositOrder.Id,
                            nameof(TransactionType.Credit),
                            depositOrder.TotalAmount,
                            depositOrder.Currency.Code,
                            nameof(TransactionOrderType.Deposit),
                            depositOrder.StatusDescription,
                            depositOrder.BankReference,
                            depositOrder.E2EId));

                    // Fee Transaction
                    await transactionEventPublisher.PublishTransactionCreatedAsync(
                        new CreateTransactionEvent(
                            depositOrder.CustomerId,
                            depositOrder.AccountId,
                            depositOrder.Id,
                            nameof(TransactionType.Debit),
                            -Math.Abs(depositOrder.FeeAmount),
                            depositOrder.Currency.Code,
                            nameof(TransactionOrderType.DepositFee),
                            nameof(TransactionOrderType.DepositFee),
                            depositOrder.BankReference,
                            depositOrder.E2EId));

                    await transaction.CommitAsync();
                    logger.LogInformation(
                        "Successfully finalized DepositOrder {OrderId} and notified",
                        depositOrder.Id);

                    return Result.Ok();
                default:
                    logger.LogCritical("Could not complete DepositOrder. Invalid status sequence. OrderId: {orderId}", depositOrder.Id);
                    return Result.Fail($"Could not complete DepositOrder. OrderId: {depositOrder.Id}");
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogCritical(
                ex,
                "Error finalizing DepositOrder {OrderId}",
                depositOrder.Id);

            return Result.Fail($"Error finalizing deposit order: {ex.Message}");
        }
    }
}