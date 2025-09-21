using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories
{
    public class QuoteOrderRepository(ServiceDbContext context)
        : Repository<QuoteOrder>(context), IQuoteOrderRepository
    {
        public async Task<IEnumerable<QuoteOrder>> GetByCustomerIdAsync(long customerId)
        {
            return await context.QuoteOrders
                .Include(x => x.BaseCurrency)
                .Include(x => x.QuoteCurrency)
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();
        }
    }
}