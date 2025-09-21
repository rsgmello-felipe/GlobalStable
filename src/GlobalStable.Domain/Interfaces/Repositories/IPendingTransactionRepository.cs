using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories
{
    public interface IPendingTransactionRepository
    {
        Task<PendingTransaction> AddAsync(PendingTransaction entity);
        Task<PendingTransaction> GetByIdAsync(long id);
        Task<IEnumerable<PendingTransaction>> GetByAccountIdAsync(long accountId);
        Task<IEnumerable<PendingTransaction>> GetByOrderIdAsync(long orderId, string orderType);
        Task RemoveAsync(PendingTransaction entity);
    }
}