using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.AccountUseCases
{
    public class DeleteAccountUseCase(IAccountRepository accountRepository)
    {
        public async Task ExecuteAsync(long accountId)
        {
            await accountRepository.RemoveByIdAsync(accountId);
        }
    }
}