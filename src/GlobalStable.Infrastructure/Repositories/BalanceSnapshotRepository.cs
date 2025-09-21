using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

public class BalanceSnapshotRepository : IBalanceSnapshotRepository
{
    private readonly ServiceDbContext _context;

    public BalanceSnapshotRepository(ServiceDbContext context)
    {
        _context = context;
    }

    public async Task<BalanceSnapshot> AddAsync(BalanceSnapshot entity)
    {
        await _context.BalanceSnapshots.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<BalanceSnapshot> GetLatestByAccountIdAsync(long accountId)
    {
        return await _context.BalanceSnapshots
            .Where(x => x.AccountId == accountId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }
}