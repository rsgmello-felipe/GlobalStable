using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> AddAsync(Transaction entity);
    Task<Transaction> GetByIdAsync(long id);
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(
        long accountId, 
        DateTime? startDate = null, 
        DateTime? endDate = null);
    Task<IEnumerable<Transaction>> GetByOrderIdAsync(long orderId, string orderType);
}