using System.Linq.Dynamic.Core;
using GlobalStable.Domain.Common;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

/// <summary>
/// Repository for handling operations related to withdrawal orders.
/// </summary>
public class WithdrawalOrderRepository(ServiceDbContext dbContext)
    : Repository<WithdrawalOrder>(dbContext), IWithdrawalOrderRepository
{
    private readonly ServiceDbContext _serviceDbContext = dbContext;

    public async Task<WithdrawalOrder?> GetByIdAsync(long withdrawalOrderId)
    {
        return await dbContext.WithdrawalOrders
            .Include(wo => wo.Currency)
            .Include(wo => wo.OrderHistory)
            .FirstOrDefaultAsync(wo => wo.Id == withdrawalOrderId);
    }

    public async Task<WithdrawalOrder?> GetByIdWithAccountAsync(long withdrawalOrderId, long accountId)
    {
        return await dbContext.WithdrawalOrders
            .Include(wo => wo.Currency)
            .Include(wo => wo.OrderHistory)
            .FirstOrDefaultAsync(wo => wo.Id == withdrawalOrderId &&
                                       wo.AccountId == accountId);
    }

    public async Task<WithdrawalOrder?> GetByIdWithHistoryAsync(long withdrawalOrderId)
    {
        return await dbContext.WithdrawalOrders
            .Include(wo => wo.Currency)
            .Include(wo => wo.OrderHistory)
            .FirstOrDefaultAsync(wo => wo.Id == withdrawalOrderId);
    }

    public async Task<Domain.Common.PagedResult<WithdrawalOrder>> GetByAccountIdWithHistoryAsync(
        long accountId,
        int page,
        int pageSize)
    {
        var query = dbContext.WithdrawalOrders
            .Include(wo => wo.Currency)
            .Include(wo => wo.OrderHistory)
            .Where(wo => wo.AccountId == accountId);

        var totalItems = await query.CountAsync();

        var orders = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new Domain.Common.PagedResult<WithdrawalOrder>(orders, new Pagination(totalItems, page, pageSize));
    }

    public async Task<Domain.Common.PagedResult<WithdrawalOrder>> GetFilteredAsync(
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
        int pageSize)
    {
        var query = dbContext.WithdrawalOrders
            .AsNoTracking()
            .Include(wo => wo.OrderHistory)
            .Include(x => x.Currency)
            .AsQueryable();

        query = query.Where(x => x.CustomerId == customerId);

        if (accountId.HasValue)
        {
            query = query.Where(x => x.AccountId == accountId.Value);
        }

        if (orderId.HasValue)
        {
            query = query.Where(x => x.Id == orderId.Value);
        }

        if (statusId.HasValue)
        {
            query = query.Where(x => x.StatusId == statusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(taxId))
        {
            query = query.Where(x => x.ReceiverTaxId != null && EF.Functions.ILike(x.ReceiverTaxId, $"%{taxId}%"));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => EF.Functions.ILike(x.ReceiverName, $"%{name}%"));
        }

        if (!string.IsNullOrWhiteSpace(e2eId))
        {
            query = query.Where(x => x.E2EId == e2eId);
        }

        if (beginDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= beginDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= endDate.Value);
        }

        var totalItems = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var direction = string.IsNullOrWhiteSpace(sortOrder) ? "ASC" : sortOrder.ToUpper();
            query = query.OrderBy($"{sortBy} {direction}");
        }
        else
        {
            query = query.OrderByDescending(t => t.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new Domain.Common.PagedResult<WithdrawalOrder>(items, new Pagination(totalItems, page, pageSize));
    }
}