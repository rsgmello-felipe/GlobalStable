using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

public class OrderStatusRepository(ServiceDbContext dbContext)
        : Repository<OrderStatus>(dbContext),
            IOrderStatusRepository
{
    public async Task<OrderStatus?> GetByIdAsync(long orderStatusId)
    {
        return await dbContext.RefOrderStatuses
            .FirstOrDefaultAsync(wo => wo.Id == orderStatusId);
    }

    public async Task<long?> GetStatusIdByNameAsync(string statusName)
    {
        return await dbContext.RefOrderStatuses
            .Where(s => EF.Functions.ILike(s.Name, $"%{statusName}%"))
            .Select(s => (long?)s.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<OrderStatus?> GetByNameAsync(string name)
    {
        return await dbContext.RefOrderStatuses
            .Where(s => EF.Functions.ILike(s.Name, $"%{name}%"))
            .FirstOrDefaultAsync();

    }

    public async Task<IEnumerable<OrderStatus>> GetAllAsync()
    {
        return await dbContext.RefOrderStatuses.ToListAsync();
    }

    public async Task<Dictionary<long, string>> GetAllAsDictionaryAsync()
    {
        return await dbContext.RefOrderStatuses
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Id, s => s.Name);
    }
}