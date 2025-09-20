using System.Linq.Dynamic.Core;
using GlobalStable.Domain.Common;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

/// <summary>
/// Repository for handling operations related to deposit orders.
/// </summary>
public class DepositOrderRepository(ServiceDbContext dbContext)
    : Repository<DepositOrder>(dbContext),
        IDepositOrderRepository
{
    public async Task<DepositOrder?> GetByIdAsync(long depositOrderId)
    {
        return await dbContext.DepositOrders
            .Include(d => d.Currency)
            .Include(d => d.OrderHistory)
            .FirstOrDefaultAsync(d => d.Id == depositOrderId);
    }

    public async Task<DepositOrder?> GetByIdWithAccountAsync(
        long depositOrderId,
        long accountId)
    {
        return await dbContext.DepositOrders
            .Include(d => d.Currency)
            .Include(d => d.OrderHistory)
            .FirstOrDefaultAsync(d =>
                d.Id == depositOrderId &&
                d.AccountId == accountId);
    }

    public async Task<Domain.Common.PagedResult<DepositOrder>> GetFilteredAsync(
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
        var query = dbContext.DepositOrders
            .Where(d => d.CustomerId == customerId)
            .AsNoTracking()
            .Include(d => d.OrderHistory)
            .Include(d => d.Currency)
            .AsQueryable();

        if (orderId.HasValue)
        {
            query = query.Where(x => x.Id == orderId.Value);
        }

        if (accountId.HasValue)
        {
            query = query.Where(x => x.AccountId == accountId.Value);
        }

        if (statusId.HasValue)
        {
            query = query.Where(x => x.StatusId == statusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(taxId))
        {
            query = query.Where(x => x.PayerTaxId != null && EF.Functions.ILike(x.PayerTaxId, $"%{taxId}%"));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.Name != null && EF.Functions.ILike(x.Name, $"%{name}%"));
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

        return new Domain.Common.PagedResult<DepositOrder>(items, new Pagination(totalItems, page, pageSize));
    }

    public async Task<Domain.Common.PagedResult<DepositOrder>> GetByAccountIdWithHistoryAsync(
        long accountId,
        int page,
        int pageSize)
    {
        var query = dbContext.DepositOrders
            .Include(d => d.Currency)
            .Include(d => d.OrderHistory)
            .Where(d => d.AccountId == accountId);

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new Domain.Common.PagedResult<DepositOrder>(items, new Pagination(totalItems, page, pageSize));
    }
}