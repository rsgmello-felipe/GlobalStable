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
    public async Task<PagedResult<T>> GetByAccountIdAsync(
        long accountId,
        int page,
        int pageSize)
    {
        var query = dbContext.Set<T>();
        var totalItems = await query.CountAsync();
        var items = await query
            .Where(e => EF.Property<long>(e, "AccountId") == accountId)
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
            .OrderByDescending(e => EF.Property<DateTime>(e, "CreatedAt"))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>(items, new Pagination(totalItems, page, pageSize));
    }
}