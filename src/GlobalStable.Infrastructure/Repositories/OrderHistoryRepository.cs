using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

    public class OrderHistoryRepository(ServiceDbContext dbContext)
        : Repository<OrderHistory>(dbContext),
            IOrderHistoryRepository
    {
        private readonly ServiceDbContext _dbContext = dbContext;

        public async Task<IEnumerable<OrderHistory>> GetWithdrawalOrderHistory(long OrderId)
        {
            return await _dbContext.OrderHistories
                .Where(s => s.WithdrawalOrderId == OrderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderHistory>> GetDepositOrderHistory(long OrderId)
        {
            return await _dbContext.OrderHistories
                .Where(s => s.DepositOrderId == OrderId)
                .ToListAsync();
        }
    }