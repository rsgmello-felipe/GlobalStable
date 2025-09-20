using System.Linq.Expressions;
using GlobalStable.Domain.Common;

namespace GlobalStable.Domain.Interfaces.Repositories;

/// <summary>
/// Generic repository interface providing common data access methods.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IRepository<T>
    where T : class
{
    Task<PagedResult<T>> GetByAccountIdAsync(
        long accountId,
        int page,
        int pageSize);

    Task AddAsync(T entity);

    Task UpdateAsync(T entity);

    Task<PagedResult<T>> GetPagedAsync(
        Expression<Func<T, bool>> filter,
        int page,
        int pageSize);
}