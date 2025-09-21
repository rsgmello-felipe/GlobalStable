using FluentResults;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.AccountUseCases
{
    public class UpdateAccountUseCase(IAccountRepository accountRepository)
    {
        public async Task<Result<Account>> ExecuteAsync(
            long customerId,
            UpdateAccountRequest request)
        {
            var account = await accountRepository.GetByCustomerIdAndIdAsync(customerId, request.AccountId);
            if (account == null) return Result.Fail<Account>("Account not found.");

            account.UpdateAccount("System", request.Name, request.WalletAddress, request.Enabled);

            await accountRepository.UpdateAsync(account);

            return Result.Ok(account);
        }
    }
}