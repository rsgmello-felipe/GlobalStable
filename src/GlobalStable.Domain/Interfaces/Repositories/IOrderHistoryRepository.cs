using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing orders history.
/// </summary>
public interface IOrderHistoryRepository : IRepository<OrderHistory>
{
    Task<IEnumerable<OrderHistory>> GetWithdrawalOrderHistory(long orderId);

    Task<IEnumerable<OrderHistory>> GetDepositOrderHistory(long OrderId);
}