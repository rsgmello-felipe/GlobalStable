using GlobalStable.Domain.Common;
using GlobalStable.Domain.DTOs;
using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing deposit orders.
/// </summary>
public interface IDepositOrderRepository : IRepository<DepositOrder>
{
    Task<DepositOrder?> GetByIdAsync(long depositOrderId);

    Task<DepositOrder?> GetByIdWithAccountAsync(
        long depositOrderId,
        long accountId);

    Task<PagedResult<DepositOrder>> GetFilteredAsync(
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

    Task<PagedResult<DepositOrder>> GetByAccountIdWithHistoryAsync(
        long accountId,
        int page,
        int pageSize);
}