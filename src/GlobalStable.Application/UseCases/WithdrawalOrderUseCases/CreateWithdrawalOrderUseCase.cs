using System.Text.Json;
using System.Text.RegularExpressions;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Helpers;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.TransactionService;
using GlobalStable.Infrastructure.Persistence;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.Withdrawal;

public class CreateWithdrawalOrderUseCase(
    IWithdrawalOrderRepository withdrawalOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    IAccountRepository accountRepository,
    IOrderEventPublisher orderEventPublisher,
    ITransactionServiceClient transactionServiceClient,
    ICustomerServiceClient customerServiceClient,
    ILogger<CreateWithdrawalOrderUseCase> logger,
    ServiceDbContext dbContext)
{
    private readonly string _includesBlockchains = "currencies_blockchains.blockchain";

    public async Task<Result<WithdrawalOrderResponse>> ExecuteAsync(
        CreateWithdrawalOrderRequest request,
        long accountId,
        string username,
        string? originHeader)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var accountResult = await GetAndValidateAccountAsync(accountId);
            if (accountResult.IsFailed) return accountResult.ToResult<WithdrawalOrderResponse>();

            var account = accountResult.Value;

            var precisionResult = ValidateAmountPrecision(request.Amount, account.Currency.Precision);
            if (precisionResult.IsFailed) return precisionResult.ToResult<WithdrawalOrderResponse>();

            var validationResult = ValidateReceiverInformation(request, account.Currency.Type);
            if (validationResult.IsFailed) return validationResult.ToResult<WithdrawalOrderResponse>();

            var blockchainResult = await ValidateBlockchainSupportIfCryptoAsync(account, request);
            if (blockchainResult.IsFailed) return blockchainResult.ToResult<WithdrawalOrderResponse>();

            var statusesResult = await GetCreatedStatusAsync();
            if (statusesResult.IsFailed) return statusesResult.ToResult<WithdrawalOrderResponse>();

            var (statuses, createdStatusId) = statusesResult.Value;

            var feeResult = await GetFeeAndAmountsAsync(account, request.Amount);
            if (feeResult.IsFailed) return feeResult.ToResult<WithdrawalOrderResponse>();

            var (feeAmount, totalAmount) = feeResult.Value;

            if(totalAmount < 0)
                return Result.Fail<WithdrawalOrderResponse>("Total amount must be greater than zero.");

            var balanceResult = await ValidateBalanceAsync(account.CustomerId, accountId, totalAmount);
            if (balanceResult.IsFailed) return balanceResult.ToResult<WithdrawalOrderResponse>();

            var withdrawalOrder = new WithdrawalOrder(
                customerId: account.CustomerId,
                accountId: accountId,
                requestedAmount: request.Amount,
                feeAmount: feeAmount,
                totalAmount: totalAmount,
                currencyId: account.CurrencyId,
                statusId: createdStatusId,
                name: request.Name,
                e2eId: null,
                receiverTaxId: request.ReceiverTaxId,
                receiverAccountKey: request.ReceiverAccountKey,
                receiverWalletAddress: request.ReceiverWalletAddress,
                blockchainNetworkId: request.ReceiverBlockchain,
                createdBy: username);

            await withdrawalOrderRepository.AddAsync(withdrawalOrder);

            var txResult = await transactionServiceClient.CreatePendingTransactionAsync(
                new CreatePendingTransactionRequest(
                    accountId,
                    -Math.Abs(withdrawalOrder.TotalAmount),
                    account.Currency.Code,
                    withdrawalOrder.Id,
                    TransactionType.Debit,
                    TransactionOrderType.Withdrawal.ToString()),
                account.CustomerId,
                accountId);

            if (!txResult.IsSuccessful)
            {
                logger.LogCritical(
                    "Failed to create pending transaction. OrderId: {orderId}. Error: {error}",
                    withdrawalOrder.Id,
                    txResult.Error?.Message);

                return Result.Fail<WithdrawalOrderResponse>(
                    $"Failed to create pending transaction. OrderId: {withdrawalOrder.Id}");
            }

            await orderEventPublisher.PublishWithdrawalOrderEvent(withdrawalOrder);

            await transaction.CommitAsync();
            var response = new WithdrawalOrderResponse(withdrawalOrder, account.Currency.Code, statuses);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogCritical(
                ex,
                "Error processing withdrawal order. AccountId: {AccountId}",
                accountId);
            return Result.Fail<WithdrawalOrderResponse>($"Error processing withdrawal order: {ex.Message}");
        }
    }

    private async Task<Result<Domain.Entities.Account>> GetAndValidateAccountAsync(long accountId)
    {
        var account = await accountRepository.GetByIdAsync(accountId);
        if (account is null)
        {
            logger.LogCritical("Account not found. AccountId: '{AccountId}'", accountId);
            return Result.Fail<Domain.Entities.Account>("Account not found.");
        }

        return Result.Ok(account);
    }

    private async Task<Result<(Dictionary<long, string> Statuses, long CreatedStatusId)>> GetCreatedStatusAsync()
    {
        var statuses = await orderStatusRepository.GetAllAsDictionaryAsync();
        if (!statuses.TryGetValueByName(OrderStatuses.Created, out var createdStatusId))
        {
            return Result.Fail<(Dictionary<long, string>, long)>(
                $"Status '{OrderStatuses.Created}' not found.");
        }

        return Result.Ok((statuses, createdStatusId));
    }

    private Result ValidateReceiverInformation(CreateWithdrawalOrderRequest request, CurrencyType currencyType)
    {
        if (currencyType == CurrencyType.Crypto)
        {
            if (string.IsNullOrWhiteSpace(request.ReceiverWalletAddress))
                return Result.Fail("ReceiverWalletAddress is required for crypto withdrawals.");

            if (string.IsNullOrWhiteSpace(request.ReceiverBlockchain))
                return Result.Fail("ReceiverBlockchain is required for crypto withdrawals.");
        }
        else if (currencyType == CurrencyType.Fiat)
        {
            if (string.IsNullOrWhiteSpace(request.ReceiverAccountKey))
                return Result.Fail("ReceiverAccountKey is required for fiat withdrawals.");

            if (string.IsNullOrWhiteSpace(request.Name))
                return Result.Fail("Name is required for fiat withdrawals.");

            if (string.IsNullOrWhiteSpace(request.ReceiverTaxId))
                return Result.Fail("ReceiverTaxId is required for fiat withdrawals.");
        }

        return Result.Ok();
    }

    private async Task<Result> ValidateBlockchainSupportIfCryptoAsync(
        Domain.Entities.Account account,
        CreateWithdrawalOrderRequest request)
    {
        if (account.Currency.Type != CurrencyType.Crypto)
            return Result.Ok();

        var response = await customerServiceClient.GetCurrencyAsync(account.Currency.Code, _includesBlockchains);

        var currencyData = response.Content?.Result?.Data?.FirstOrDefault();
        if (currencyData?.CurrenciesBlockchains is null || !currencyData.CurrenciesBlockchains.Any())
            return Result.Fail("Blockchain not supported for selected currency.");

        var supportedBlockchain = currencyData.CurrenciesBlockchains
            .FirstOrDefault(b => b.Blockchain.Name.Equals(request.ReceiverBlockchain, StringComparison.OrdinalIgnoreCase));

        if (supportedBlockchain is null)
            return Result.Fail("Blockchain not supported for selected currency.");

        if (string.IsNullOrWhiteSpace(request.ReceiverWalletAddress))
            return Result.Fail("Wallet address is required.");

        var regexPattern = supportedBlockchain.Blockchain.Regex;

        if (!Regex.IsMatch(request.ReceiverWalletAddress, regexPattern))
            return Result.Fail("Invalid wallet address format for selected blockchain.");

        return Result.Ok();
    }


    private async Task<Result<(decimal FeeAmount, decimal TotalAmount)>> GetFeeAndAmountsAsync(
        Account account,
        decimal amount)
    {
        var feeAmount = (amount * account.WithdrawalPercentageFee) + account.WithdrawalFlatFee;
        var totalAmount = Math.Round(amount + feeAmount, account.Currency.Precision);

        return Result.Ok((feeAmount, totalAmount));
    }

    private async Task<Result> ValidateBalanceAsync(
        long customerId,
        long accountId,
        decimal totalAmount)
    {
        var balanceResponse = await transactionServiceClient.GetBalanceAsync(customerId, accountId);
        if (!balanceResponse.IsSuccessful)
        {
            logger.LogCritical(
                "Failed to retrieve balance. AccountId: {AccountId}. Error: {Error}",
                accountId,
                balanceResponse.Error.Message);

            return Result.Fail($"Could not retrieve balance for accountId: {accountId}");
        }

        if (balanceResponse.Content.Result?.Balance - totalAmount < 0)
        {
            logger.LogWarning("Insufficient balance to process order. AccountId: {AccountId}", accountId);
            return Result.Fail("Insufficient balance to process order.");
        }

        return Result.Ok();
    }

    private Result ValidateAmountPrecision(decimal amount, int allowedPrecision)
    {
        var actualPrecision = BitConverter.GetBytes(decimal.GetBits(amount)[3])[2];

        if (actualPrecision > allowedPrecision)
        {
            return Result.Fail($"Amount precision exceeds the allowed limit of {allowedPrecision} decimal places.");
        }

        return Result.Ok();
    }
}
