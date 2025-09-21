using FluentResults;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;
using GlobalStable.Infrastructure.Persistence;
using GlobalStable.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GlobalStable.Application.UseCases.DepositUseCases;

public class CreateDepositOrderUseCase(
    IRepository<DepositOrder> depositOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    IAccountRepository accountRepository,
    IBrlProviderClient brlProviderClient,
    ILogger<CreateDepositOrderUseCase> logger,
    IOptions<CallbackSettings> callbackSettings,
    ServiceDbContext dbContext)
{
    public async Task<Result<DepositOrderResponse>> ExecuteAsync(
        CreateDepositOrderRequest request,
        long accountId,
        string userId,
        string? originHeader)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var account = await accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                logger.LogCritical("Account not found! AccountId: '{}'", accountId);
                return Result.Fail<DepositOrderResponse>("Account not found.");
            }

            var pendingStatus = await orderStatusRepository.GetByNameAsync(OrderStatuses.PendingDeposit);
            if (pendingStatus == null)
            {
                logger.LogCritical("Status '{pending}' not found.", OrderStatuses.PendingDeposit);
                return Result.Fail<DepositOrderResponse>($"Status '{OrderStatuses.PendingDeposit}' not found.");
            }

            var feeAmount = (request.Amount * account.DepositPercentageFee) + account.DepositFlatFee;
            var totalAmount = Math.Round(request.Amount + feeAmount, account.Currency.Precision);
            var expireAt = DateTimeOffset.Now.AddSeconds(request.Expiration);

            var depositOrder = new DepositOrder(
                customerId: account.CustomerId,
                accountId: accountId,
                requestedAmount: request.Amount,
                feeAmount: feeAmount,
                totalAmount: totalAmount,
                currencyId: account.CurrencyId,
                statusId: pendingStatus.Id,
                bankReference: null,
                expireAt: expireAt,
                createdBy: userId,
                e2eId: null,
                statusDescription: null);

            await depositOrderRepository.AddAsync(depositOrder);

            var bgpRequest = new BrlProviderCreateDepositRequest(
                depositOrder.Id,
                account.Currency.Code,
                depositOrder.TotalAmount,
                request.Expiration);

            var bgpResponse = await brlProviderClient.CreateDepositOrderAsync(bgpRequest);

            if (!bgpResponse.IsSuccessful || bgpResponse.Content?.Result is null)
            {
                logger.LogError("Failed to create deposit order in connector. Error: {error}", bgpResponse?.Error?.Content);
                await transaction.RollbackAsync();
                return Result.Fail("Could not create deposit order");
            }

            depositOrder.UpdateBankTransactionInformation(bgpResponse.Content.Result.ReferenceId);

            await depositOrderRepository.UpdateAsync(depositOrder);

            await transaction.CommitAsync();

            var statuses = await orderStatusRepository.GetAllAsDictionaryAsync();

            var response = new DepositOrderResponse(
                depositOrder,
                account.Currency.Code,
                statuses);

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error creating deposit order.");
            await transaction.RollbackAsync();
            return Result.Fail<DepositOrderResponse>($"Error creating deposit order: {ex.Message}");
        }
    }
}
