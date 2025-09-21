using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ServiceDbContext _context;

    public TransactionRepository(ServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction> AddAsync(Transaction entity)
    {
        await _context.Transactions.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(
        long accountId, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.Transactions
            .Where(x => x.AccountId == accountId);

        if (startDate.HasValue)
            query = query.Where(x => x.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.CreatedAt <= endDate.Value);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}