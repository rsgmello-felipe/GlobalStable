using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

public class TransactionRepository(ServiceDbContext context) 
    : Repository<Transaction>(context), ITransactionRepository
{
    public async Task<IEnumerable<Transaction>> GetByOrderIdAsync(
        long accountId, 
        long orderId)
    {
        return await context.Transactions
            .Where(t => 
                t.AccountId == accountId &&
                t.OrderId == orderId)
            .ToListAsync();

    }
}