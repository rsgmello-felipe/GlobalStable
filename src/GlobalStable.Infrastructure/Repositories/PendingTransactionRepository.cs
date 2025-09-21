using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

public class PendingTransactionRepository : IPendingTransactionRepository
{
    private readonly ServiceDbContext _context;

    public PendingTransactionRepository(ServiceDbContext context)
    {
        _context = context;
    }

    public async Task<PendingTransaction> AddAsync(PendingTransaction entity)
    {
        await _context.PendingTransactions.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<PendingTransaction>> GetByAccountIdAsync(long accountId)
    {
        return await _context.PendingTransactions
            .Where(x => x.AccountId == accountId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task RemoveAsync(PendingTransaction entity)
    {
        _context.PendingTransactions.Remove(entity);
        await _context.SaveChangesAsync();
    }
}