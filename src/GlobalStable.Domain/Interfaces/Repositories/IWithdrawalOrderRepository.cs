using GlobalStable.Domain.Common;
using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing withdrawal orders.
/// </summary>
public interface IWithdrawalOrderRepository : IRepository<WithdrawalOrder>
{
    Task<WithdrawalOrder?> GetByIdAsync(long withdrawalOrderId);

    Task<WithdrawalOrder?> GetByIdWithAccountAsync(long withdrawalOrderId, long accountId);

    Task<PagedResult<WithdrawalOrder>> GetByAccountIdWithHistoryAsync(
        long accountId,
        int page,
        int pageSize);

    Task<PagedResult<WithdrawalOrder>> GetFilteredAsync(
        long customerId,
        long? orderId,
        long? accountId,
        long? statusId,
        string? name,
        string? e2eId,
        string? taxId,
        DateTime? beginDate,
        DateTime? endDate,
        string? sortBy,
        string? sortOrder,
        int page,
        int pageSize);
}