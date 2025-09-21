using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.AccountUseCases;

public class GetAccountsByCustomerUseCase(IAccountRepository accountRepository)
{
    public async Task<IEnumerable<Account?>> ExecuteAsync(long customerId)
    {
        var customerAccounts = await accountRepository.GetByCustomerId(customerId);

        return customerAccounts;
    }
}