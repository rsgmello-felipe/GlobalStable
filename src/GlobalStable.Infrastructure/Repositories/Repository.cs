using System.Linq.Expressions;
using GlobalStable.Domain.Common;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation providing reusable data access methods.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class Repository<T>(ServiceDbContext dbContext)
    : IRepository<T>
    where T : class
{
    public async Task<T?> GetByIdAsync(long id)
    {
        return await dbContext.Set<T>()
            .AsNoTracking()
            .SingleOrDefaultAsync(e => EF.Property<long>(e, "Id") == id);
    }
    
    public async Task<T?> GetByCustomerIdAndIdAsync(long id, long customerId)
    {
        return await dbContext.Set<T>()
            .AsNoTracking()
            .SingleOrDefaultAsync(e => 
                EF.Property<long>(e, "Id") == id &&
                EF.Property<long>(e, "CustomerId") == customerId);
    }
    
    public async Task<PagedResult<T>> GetByAccountIdAsync(
        long accountId,
        int page,
        int pageSize)
    {
        var query = dbContext.Set<T>().Where(e => EF.Property<long>(e, "AccountId") == accountId);
        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => EF.Property<DateTime>(e, "CreatedAt"))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>(items, new Pagination(totalItems, page, pageSize));
    }

    public async Task AddAsync(T entity)
    {
        await dbContext.Set<T>().AddAsync(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        dbContext.Set<T>().Update(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task<PagedResult<T>> GetPagedAsync(
        Expression<Func<T, bool>> filter,
        int page,
        int pageSize)
    {
        var query = dbContext.Set<T>().Where(filter);

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => EF.Property<DateTimeOffset>(e, "CreatedAt"))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>(items, new Pagination(totalItems, page, pageSize));
    }
}