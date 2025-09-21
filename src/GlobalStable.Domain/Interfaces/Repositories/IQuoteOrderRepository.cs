using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories
{
    public interface IQuoteOrderRepository : IRepository<QuoteOrder>
    {
        Task<IEnumerable<QuoteOrder>> GetByCustomerIdAsync(long customerId);
    }
}