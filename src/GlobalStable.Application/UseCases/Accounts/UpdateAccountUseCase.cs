using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.Accounts
{
    public class UpdateAccountUseCase
    {
        private readonly IAccountRepository _accountRepository;

        public UpdateAccountUseCase(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<Account> ExecuteAsync(long accountId, string name, string type, decimal balance)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) throw new Exception("Account not found");

            account.Update(name, type, balance);

            await _accountRepository.UpdateAsync(account);

            return account;
        }
    }
}