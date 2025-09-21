using GlobalStable.Application.ApiRequests;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.AccountUseCases
{
    public class UpdateAccountUseCase(IAccountRepository accountRepository)
    {
        public async Task<Account> ExecuteAsync(UpdateAccountRequest request)
        {
            var account = await accountRepository.GetByIdAsync(request.AccountId);
            if (account == null) throw new Exception("Account not found");

            account.UpdateAccount("System", request.Name, request.WalletAddress, request.Enabled);

            await accountRepository.UpdateAsync(account);

            return account;
        }
    }
}