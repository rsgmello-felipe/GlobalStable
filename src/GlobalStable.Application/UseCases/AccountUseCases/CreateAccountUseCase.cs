using FluentResults;
using GlobalStable.Application.UseCases.Accounts;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.AccountUseCases
{
    public class CreateAccountUseCase(
        IAccountRepository accountRepository,
        ICurrencyRepository currencyRepository,
        ILogger<CreateAccountUseCase> logger
        )
    {
        public async Task<Result<Account>> ExecuteAsync(
            long customerId,
            CreateAccountRequest request)
        {
            var currency = await currencyRepository.GetByCodeAsync(request.Currency);
            
            var accountExists = await accountRepository.CheckIfAccountExists(customerId, currency.Id);
            if (accountExists)
            {
                logger.LogInformation(
                    "Account with currency '{currency}' already exists for customerId: '{customerId}'",
                    request.Currency,
                    customerId);
                return Result.Fail($"Account with currency '{request.Currency}' already exists for customerId: '{customerId}'");
            }
            
            var account = new Account(
                request.Name,
                customerId,
                currency.Id,
                request.WithdrawalPercentageFee,
                request.WithdrawalFlatFee,
                request.DepositPercentageFee,
                request.DepositFlatFee,
                "System",
                request.WalletAddress);

            await accountRepository.AddAsync(account);
            return account;
        }
    }
}