using GlobalStable.Domain.Common;
using GlobalStable.Domain.DTOs;
using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing deposit orders.
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    Task<IEnumerable<Account?>> GetByCustomerId(long customerId);

    Task<bool> CheckIfAccountExists(long customerId, long currencyId);
}