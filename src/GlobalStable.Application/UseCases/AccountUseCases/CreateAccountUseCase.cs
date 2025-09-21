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
        public async Task<Result<Account>> ExecuteAsync(CreateAccountRequest request)
        {
            var currency = await currencyRepository.GetByCodeAsync(request.Currency);
            
            var accountExists = await accountRepository.CheckIfAccountExists(request.CustomerId, currency.Id);
            if (accountExists)
            {
                logger.LogInformation(
                    "Account with currency '{currency}' already exists for customerId: '{customerId}'",
                    request.Currency,
                    request.CustomerId);
                return Result.Fail($"Account with currency '{request.Currency}' already exists for customerId: '{request.CustomerId}'");
            }
            
            var account = new Account(
                request.Name,
                request.CustomerId,
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