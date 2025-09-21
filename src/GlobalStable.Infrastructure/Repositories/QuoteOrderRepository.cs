using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories
{
    public class QuoteOrderRepository : IQuoteOrderRepository
    {
        private readonly ServiceDbContext _context;

        public QuoteOrderRepository(ServiceDbContext context)
        {
            _context = context;
        }

        public async Task<QuoteOrder> AddAsync(QuoteOrder entity)
        {
            await _context.QuoteOrders.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<QuoteOrder> GetByIdAsync(long id)
        {
            return await _context.QuoteOrders
                .Include(x => x.BaseCurrency)
                .Include(x => x.QuoteCurrency)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<QuoteOrder>> GetAllAsync()
        {
            return await _context.QuoteOrders
                .Include(x => x.BaseCurrency)
                .Include(x => x.QuoteCurrency)
                .ToListAsync();
        }

        public async Task<QuoteOrder> UpdateAsync(QuoteOrder entity)
        {
            _context.QuoteOrders.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task RemoveAsync(QuoteOrder entity)
        {
            _context.QuoteOrders.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<QuoteOrder>> GetByCustomerIdAsync(long customerId)
        {
            return await _context.QuoteOrders
                .Include(x => x.BaseCurrency)
                .Include(x => x.QuoteCurrency)
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuoteOrder>> GetByStatusAsync(int statusId)
        {
            return await _context.QuoteOrders
                .Include(x => x.BaseCurrency)
                .Include(x => x.QuoteCurrency)
                .Where(x => x.StatusId == statusId)
                .ToListAsync();
        }
    }
}