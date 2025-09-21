using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByOrderIdAsync(
        long accountId,
        long orderId);
}