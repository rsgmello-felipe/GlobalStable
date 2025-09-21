using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.Accounts
{
    public class CreateAccountUseCase
    {
        private readonly IAccountRepository _accountRepository;

        public CreateAccountUseCase(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<Account> ExecuteAsync(string name, string type, decimal initialBalance)
        {
            var account = new Account(name, type, initialBalance);

            await _accountRepository.AddAsync(account);
            return account;
        }
    }
}