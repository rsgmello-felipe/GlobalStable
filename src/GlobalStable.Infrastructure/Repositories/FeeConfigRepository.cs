using GlobalStable.Domain.Common;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

/// <summary>
/// Repository for handling operations related to fee configurations.
/// </summary>
public class FeeConfigRepository(ServiceDbContext dbContext)
    : Repository<FeeConfig>(dbContext),
        IFeeConfigRepository
{
    public async Task<IEnumerable<FeeConfig>?> GetAllByAccountIdAsync(
        long accountId)
    {
        var feeConfig = await dbContext.FeeConfigs
            .Where(fc => fc.AccountId == accountId)
            .ToListAsync();

        return feeConfig;
    }

    public async Task<FeeConfig?> GetByAccountIdAsync(
        long accountId,
        TransactionOrderType transactionOrderType)
    {
        var feeConfig = await dbContext.FeeConfigs
            .Where(
                fc => fc.AccountId == accountId &&
                      fc.TransactionOrderType == transactionOrderType &&
                      fc.Enabled)
            .OrderByDescending(fc => fc.CreatedAt)
            .FirstOrDefaultAsync();

        return feeConfig;
    }

    public async Task<List<FeeConfig>> GetEnabledByAccountIdAsync(long accountId)
    {
        return await dbContext.FeeConfigs
            .Where(f => f.AccountId == accountId && f.Enabled)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<FeeConfig> configs)
    {
        dbContext.FeeConfigs.AddRange(configs);
        await dbContext.SaveChangesAsync();
    }
}