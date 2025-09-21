using GlobalStable.Domain.Common;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

/// <summary>
/// Repository for handling operations related to fee configurations.
/// </summary>
public class AccountRepository(ServiceDbContext dbContext)
    : Repository<Account>(dbContext),
        IAccountRepository
{
    public async Task<IEnumerable<Account?>> GetByCustomerId(long customerId)
    {
        var account = await dbContext.Accounts
            .AsNoTracking()
            .Include(a => a.Currency)
            .Where(a => a.CustomerId == customerId && a.Enabled)
            .ToListAsync();

        return account;
    }
    
    public async Task<bool> CheckIfAccountExists(long customerId, long currencyId)
    {
        var accountExists = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => 
                a.CustomerId == customerId && 
                a.CurrencyId == currencyId && 
                a.Enabled)
            .AnyAsync();

        return accountExists;
    }
}