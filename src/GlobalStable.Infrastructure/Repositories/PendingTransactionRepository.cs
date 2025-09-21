using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

public class PendingTransactionRepository(ServiceDbContext context)
    : Repository<PendingTransaction>(context), IPendingTransactionRepository
{
    public async Task<IEnumerable<PendingTransaction>> GetByOrderIdAsync(long accountId, long orderId)
    {
        return await context.PendingTransactions
            .Where(pt => 
                pt.AccountId == accountId &&
                pt.OrderId == orderId)
            .ToListAsync();
    }
}