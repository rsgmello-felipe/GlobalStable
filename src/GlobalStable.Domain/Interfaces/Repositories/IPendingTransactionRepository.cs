using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories
{
    public interface IPendingTransactionRepository : IRepository<PendingTransaction>
    {
        Task<IEnumerable<PendingTransaction>> GetByOrderIdAsync(long accountId, long orderId);
    }
}