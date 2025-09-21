using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

public interface IBalanceSnapshotRepository
{
    Task<BalanceSnapshot> AddAsync(BalanceSnapshot entity);
    Task<BalanceSnapshot> GetByIdAsync(long id);
    Task<BalanceSnapshot> GetLatestByAccountIdAsync(long accountId);
    Task<IEnumerable<BalanceSnapshot>> GetByAccountIdAsync(long accountId);
}