using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

public interface IBalanceSnapshotRepository : IRepository<BalanceSnapshot>
{
    Task<BalanceSnapshot?> GetLatestByAccountIdAsync(long accountId);
}