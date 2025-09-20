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
    public async Task<Account?> GetByIdAsync(long id)
    {
        var account = await dbContext.Accounts
            .Include(a => a.Currency)
            .FirstOrDefaultAsync(a => a.Id == id);

        return account;
    }
}