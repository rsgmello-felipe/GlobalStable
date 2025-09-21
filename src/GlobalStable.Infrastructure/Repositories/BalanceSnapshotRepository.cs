using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

public class BalanceSnapshotRepository(ServiceDbContext context) 
    : Repository<BalanceSnapshot>(context), IBalanceSnapshotRepository
{
    public async Task<BalanceSnapshot?> GetLatestByAccountIdAsync(long accountId)
    {
        return await context.BalanceSnapshots
            .Where(bs => bs.AccountId == accountId)
            .OrderByDescending(bs => bs.CreatedAt)
            .FirstOrDefaultAsync();
    }
}