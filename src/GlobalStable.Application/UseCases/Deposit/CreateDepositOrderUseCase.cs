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
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GlobalStable.Application.UseCases.Deposit;

public class CreateDepositOrderUseCase(
    IRepository<DepositOrder> depositOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    IFeeConfigRepository feeConfigRepository,
    IAccountRepository accountRepository,
    IBgpConnectorClient bgpConnectorClient,
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

            var depositFee = await feeConfigRepository.GetByAccountIdAsync(accountId, TransactionOrderType.Deposit);
            if (depositFee == null)
            {
                logger.LogCritical("Fee config not found. AccountId: {accountId}", accountId);
                return Result.Fail<DepositOrderResponse>("Fee config not found.");
            }

            var feeAmount = (request.Amount * depositFee.FeePercentage) + depositFee.FlatFee;
            var totalAmount = Math.Round(request.Amount + feeAmount, account.Currency.Precision);
            var expireAt = DateTimeOffset.Now.AddSeconds(request.Expiration);

            var depositOrder = new DepositOrder(
                customerId: account.CustomerId,
                accountId: accountId,
                isAutomated: false,
                requestedAmount: request.Amount,
                feeAmount: feeAmount,
                totalAmount: totalAmount,
                currencyId: account.CurrencyId,
                statusId: pendingStatus.Id,
                bankReference: null,
                origin: originHeader ?? OriginHeaders.Api,
                webhookUrl: request.WebhookUrl,
                expireAt: expireAt,
                createdBy: userId,
                bankId: null,
                payerTaxId: request.PayerTaxId,
                e2eId: null,
                statusDescription: null,
                name: request.Name);

            await depositOrderRepository.AddAsync(depositOrder);

            var bgpRequest = new BgpCreateDepositRequest(
                depositOrder.Id,
                account.Currency.Code,
                depositOrder.TotalAmount,
                request.Expiration,
                depositOrder.Name,
                depositOrder.PayerTaxId,
                callbackSettings.Value.DepositUrl);

            var bgpResponse = await bgpConnectorClient.CreateDepositOrderAsync(bgpRequest);

            if (!bgpResponse.IsSuccessful || bgpResponse.Content?.Result is null)
            {
                logger.LogError("Failed to create deposit order in connector. Error: {error}", bgpResponse?.Error?.Content);
                await transaction.RollbackAsync();
                return Result.Fail("Could not create deposit order");
            }

            depositOrder.UpdateBankTransactionInformation(
                referenceId: bgpResponse.Content.Result.ReferenceId,
                pixCopyPaste: bgpResponse.Content.Result.PixCopyPaste,
                cvu: bgpResponse.Content.Result.Cvu);

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
