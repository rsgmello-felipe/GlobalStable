using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.Accounts
{
    public class DeleteAccountUseCase
    {
        private readonly IAccountRepository _accountRepository;

        public DeleteAccountUseCase(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task ExecuteAsync(long accountId)
        {
            // Verifica se a conta existe
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) throw new Exception("Account not found");

            // Remove a conta
            await _accountRepository.RemoveAsync(account);
        }
    }
}