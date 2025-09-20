using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing fee configurations.
/// </summary>
public interface IFeeConfigRepository : IRepository<FeeConfig>
{
    Task<IEnumerable<FeeConfig>?> GetAllByAccountIdAsync(
        long accountId);

    Task<FeeConfig?> GetByAccountIdAsync(
        long accountId,
        TransactionOrderType transactionOrderType);

    Task<List<FeeConfig>> GetEnabledByAccountIdAsync(long accountId);

    Task AddRangeAsync(IEnumerable<FeeConfig> configs);
}